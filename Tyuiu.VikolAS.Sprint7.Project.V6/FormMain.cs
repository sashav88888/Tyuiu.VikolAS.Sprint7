using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tyuiu.VikolAS.Sprint7.Project.V6.Lib;

namespace Tyuiu.VikolAS.Sprint7.Project.V6
{
    // Главное окно приложения. Интерфейс создаётся программно, без дизайнера.
    public partial class FormMain : Form
    {
        private readonly DataService _dataService = new DataService();
        private readonly string _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patients.csv");

        // Контролы
        private DataGridView dataGridViewPatients = null!;
        private TextBox textBoxSearch = null!;
        private ComboBox comboBoxSort = null!;
        private ComboBox comboBoxFilterDiagnosis = null!;
        private Label labelStats = null!;
        private Button buttonRefreshStats = null!;
        private Panel panelChart = null!;

        // Храним последний гистограммный словарь для перерисовки
        private Dictionary<string, int> _lastHistogram = new Dictionary<string, int>();

        public FormMain()
        {
            Text = "Поликлиника - Tyuiu_VikolAS";
            Name = "FormMain_VikolAS";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            CreateUi();
            Load += FormMain_Load;
        }

        // Создать UI: меню, панель инструментов, таблица, фильтры и график
        private void CreateUi()
        {
            // Меню
            var menu = new MenuStrip { Dock = DockStyle.Top };
            menu.Name = "menuMain_VikolAS";
            var menuFile = new ToolStripMenuItem("Файл") { Name = "menuFile_VikolAS" };
            var menuOpen = new ToolStripMenuItem("Открыть", null, (s, e) => LoadData()) { Name = "menuOpen_VikolAS" };
            var menuSave = new ToolStripMenuItem("Сохранить", null, (s, e) => SaveData()) { Name = "menuSave_VikolAS" };
            var menuExit = new ToolStripMenuItem("Выход", null, (s, e) => Close()) { Name = "menuExit_VikolAS" };
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuOpen, menuSave, new ToolStripSeparator(), menuExit });

            var menuHelp = new ToolStripMenuItem("Справка") { Name = "menuHelp_VikolAS" };
            var menuAbout = new ToolStripMenuItem("О программе", null, (s, e) => MessageBox.Show("Учебная программа. Хранение пациентов в CSV.", "О программе")) { Name = "menuAbout_VikolAS" };
            var menuGuide = new ToolStripMenuItem("Краткое руководство", null, (s, e) => MessageBox.Show("Добавьте пациента кнопкой 'Добавить'. Редактирование — выбрать строку и нажать 'Изменить'. Фильтры слева.", "Руководство")) { Name = "menuGuide_VikolAS" };
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout, menuGuide });

            menu.Items.AddRange(new ToolStripItem[] { menuFile, menuHelp });
            Controls.Add(menu);

            // Панель инструментов
            var tool = new ToolStrip { Dock = DockStyle.Top };
            tool.Name = "toolStrip_VikolAS";
            var buttonLoad = new ToolStripButton("Загрузить") { ToolTipText = "Загрузить данные из CSV", Name = "buttonLoad_VikolAS" };
            var buttonSave = new ToolStripButton("Сохранить") { ToolTipText = "Сохранить данные в CSV", Name = "buttonSave_VikolAS" };
            var buttonAdd = new ToolStripButton("Добавить") { ToolTipText = "Добавить пациента", Name = "buttonAdd_VikolAS" };
            var buttonEdit = new ToolStripButton("Изменить") { ToolTipText = "Изменить выбранного пациента", Name = "buttonEdit_VikolAS" };
            var buttonDelete = new ToolStripButton("Удалить") { ToolTipText = "Удалить выбранного пациента", Name = "buttonDelete_VikolAS" };
            buttonLoad.Click += (s, e) => LoadData();
            buttonSave.Click += (s, e) => SaveData();
            buttonAdd.Click += (s, e) => ShowEditDialog(null);
            buttonEdit.Click += (s, e) => { if (dataGridViewPatients.SelectedRows.Count > 0) ShowEditDialog(GetSelectedPatient()); };
            buttonDelete.Click += (s, e) => { if (dataGridViewPatients.SelectedRows.Count > 0) { _dataService.DeletePatient(GetSelectedPatient().Id); RefreshControlsAfterLoad(); } };
            tool.Items.AddRange(new ToolStripItem[] { buttonLoad, buttonSave, new ToolStripSeparator(), buttonAdd, buttonEdit, buttonDelete });
            Controls.Add(tool);

            // Главная область
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            split.Name = "splitMain_VikolAS";
            split.SplitterDistance = 320;
            Controls.Add(split);

            // Верх: грид и панель фильтров/управления
            var panelTop = new Panel { Dock = DockStyle.Fill };
            panelTop.Name = "panelTop_VikolAS";
            split.Panel1.Controls.Add(panelTop);

            dataGridViewPatients = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
            dataGridViewPatients.Name = "dataGridViewPatients_VikolAS";
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Width = 50, Name = "colId_VikolAS" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = "LastName", Name = "colLastName_VikolAS" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = "FirstName", Name = "colFirstName_VikolAS" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = "MiddleName", Name = "colMiddleName_VikolAS" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата р.", DataPropertyName = "BirthDate", Name = "colBirthDate_VikolAS" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Врач", DataPropertyName = "DoctorFullName", Name = "colDoctor_VikolAS" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Диагноз", DataPropertyName = "Diagnosis", Name = "colDiagnosis_VikolAS" });

            panelTop.Controls.Add(dataGridViewPatients);

            var panelTopRight = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 320, FlowDirection = FlowDirection.TopDown, Padding = new Padding(8) };
            panelTopRight.Name = "panelTopRight_VikolAS";
            panelTop.Controls.Add(panelTopRight);

            // Поиск
            var labelSearch = new Label { Text = "Поиск по фамилии:", AutoSize = true, Name = "labelSearch_VikolAS" };
            textBoxSearch = new TextBox { Width = 280, Name = "textBoxSearch_VikolAS" };
            textBoxSearch.TextChanged += (s, e) => RefreshGrid();
            panelTopRight.Controls.Add(labelSearch);
            panelTopRight.Controls.Add(textBoxSearch);

            // Сортировка
            var labelSort = new Label { Text = "Сортировка:", AutoSize = true, Name = "labelSort_VikolAS" };
            comboBoxSort = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Name = "comboBoxSort_VikolAS" };
            comboBoxSort.Items.AddRange(new string[] { "Id", "Фамилия", "Возраст" });
            comboBoxSort.SelectedIndex = 1;
            comboBoxSort.SelectedIndexChanged += (s, e) => RefreshGrid();
            panelTopRight.Controls.Add(labelSort);
            panelTopRight.Controls.Add(comboBoxSort);

            // Фильтр по диагнозу
            var labelFilter = new Label { Text = "Фильтр по диагнозу:", AutoSize = true, Name = "labelFilter_VikolAS" };
            comboBoxFilterDiagnosis = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDown, Name = "comboBoxFilterDiagnosis_VikolAS" };
            comboBoxFilterDiagnosis.TextChanged += (s, e) => RefreshGrid();
            panelTopRight.Controls.Add(labelFilter);
            panelTopRight.Controls.Add(comboBoxFilterDiagnosis);

            // Нижняя часть: статистика и график
            var panelBottom = new Panel { Dock = DockStyle.Fill, Name = "panelBottom_VikolAS" };
            split.Panel2.Controls.Add(panelBottom);

            var statsPanel = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 300, FlowDirection = FlowDirection.TopDown, Padding = new Padding(8), Name = "statsPanel_VikolAS" };
            panelBottom.Controls.Add(statsPanel);

            labelStats = new Label { Text = "Статистика:", AutoSize = true, Name = "labelStats_VikolAS" };
            statsPanel.Controls.Add(labelStats);

            buttonRefreshStats = new Button { Text = "Обновить статистику", Width = 260, Name = "buttonRefreshStats_VikolAS" };
            buttonRefreshStats.Click += (s, e) => UpdateStatistics();
            statsPanel.Controls.Add(buttonRefreshStats);

            panelChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Name = "panelChart_VikolAS" };
            panelChart.Paint += PanelChart_Paint;
            panelBottom.Controls.Add(panelChart);

            var toolTip = new ToolTip();
            toolTip.SetToolTip(dataGridViewPatients, "Выберите строку, чтобы изменить или удалить пациента");
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _dataService.LoadFromCsv(_dataPath);
                RefreshControlsAfterLoad();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message);
            }
        }

        private void SaveData()
        {
            try
            {
                _dataService.SaveToCsv(_dataPath);
                MessageBox.Show("Данные сохранены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void RefreshControlsAfterLoad()
        {
            var diagnoses = _dataService.Patients.Select(x => x.Diagnosis).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            comboBoxFilterDiagnosis.Items.Clear();
            comboBoxFilterDiagnosis.Items.AddRange(diagnoses);
            RefreshGrid();
            UpdateStatistics();
        }

        private void RefreshGrid()
        {
            var list = _dataService.Patients.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(textBoxSearch.Text))
                list = list.Where(x => x.LastName.IndexOf(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(comboBoxFilterDiagnosis.Text))
                list = list.Where(x => string.Equals(x.Diagnosis, comboBoxFilterDiagnosis.Text, StringComparison.OrdinalIgnoreCase));
            var key = comboBoxSort.SelectedItem?.ToString() ?? "Id";
            list = key switch
            {
                "Фамилия" => list.OrderBy(x => x.LastName),
                "Возраст" => list.OrderBy(x => x.Age),
                _ => list.OrderBy(x => x.Id),
            };

            dataGridViewPatients.DataSource = list.Select(x => new
            {
                x.Id,
                x.LastName,
                x.FirstName,
                x.MiddleName,
                BirthDate = x.BirthDate.ToShortDateString(),
                x.DoctorFullName,
                x.Diagnosis
            }).ToList();

            UpdateStatistics();
            UpdateChart();
        }

        private void UpdateStatistics()
        {
            var stats = _dataService.GetStatistics();
            labelStats.Text = $"Статистика: всего={stats.Count}  ср.возраст={Math.Round(stats.AverageAge, 1)}  мин={stats.MinAge} макс={stats.MaxAge}";
        }

        private void UpdateChart()
        {
            _lastHistogram = _dataService.GetHistogramByDiagnosis();
            panelChart.Invalidate();
        }

        private void PanelChart_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(panelChart.BackColor);
            if (_lastHistogram == null || _lastHistogram.Count == 0)
            {
                using var f = new Font("Arial", 10);
                g.DrawString("Нет данных для графика", f, Brushes.Black, new PointF(10, 10));
                return;
            }

            int margin = 20;
            int w = panelChart.ClientSize.Width - margin * 2;
            int h = panelChart.ClientSize.Height - margin * 2;
            int bars = _lastHistogram.Count;
            int max = _lastHistogram.Values.Max();
            int barWidth = Math.Max(20, w / (bars * 2));
            int spacing = barWidth; // простая схема
            int x = margin;
            var keys = _lastHistogram.Keys.ToList();
            using var font = new Font("Arial", 9);

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                int val = _lastHistogram[key];
                int barHeight = max == 0 ? 0 : (int)(h * ((double)val / max));
                var rect = new Rectangle(x, margin + (h - barHeight), barWidth, barHeight);
                g.FillRectangle(Brushes.SteelBlue, rect);
                g.DrawRectangle(Pens.Black, rect);
                // подпись
                var labelRect = new Rectangle(x - 10, margin + h + 4, barWidth + 20, 40);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near }; 
                g.DrawString(key, font, Brushes.Black, labelRect, sf);
                // значение над столбиком
                var valStr = val.ToString();
                var valSize = g.MeasureString(valStr, font);
                g.DrawString(valStr, font, Brushes.Black, x + barWidth/2 - valSize.Width/2, margin + (h - barHeight) - valSize.Height - 2);

                x += barWidth + spacing;
            }
        }

        private Patient GetSelectedPatient()
        {
            if (dataGridViewPatients.SelectedRows.Count == 0) return new Patient();
            var idObj = dataGridViewPatients.SelectedRows[0].Cells[0].Value;
            if (idObj == null) return new Patient();
            int id = Convert.ToInt32(idObj);
            return _dataService.Patients.First(x => x.Id == id);
        }

        private void ShowEditDialog(Patient? p)
        {
            var isNew = p == null;
            var copy = p == null ? new Patient() : new Patient
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                MiddleName = p.MiddleName,
                BirthDate = p.BirthDate,
                DoctorFullName = p.DoctorFullName,
                DoctorPosition = p.DoctorPosition,
                Diagnosis = p.Diagnosis,
                Ambulatory = p.Ambulatory,
                SickLeaveDays = p.SickLeaveDays,
                OnDispensary = p.OnDispensary,
                Note = p.Note
            };
            using var edit = new FormEditPatient(copy);
            if (edit.ShowDialog() == DialogResult.OK)
            {
                if (isNew)
                    _dataService.AddPatient(edit.Patient);
                else
                    _dataService.UpdatePatient(edit.Patient);
                RefreshControlsAfterLoad();
            }
        }
    }
}
