using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Tyuiu.VikolAS.Sprint7.Project.V6.Lib;

namespace Tyuiu.VikolAS.Sprint7.Project.V6
{
    public partial class FormMain : Form
    {
        private readonly DataService _dataService = new DataService();
        private readonly string _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patients.csv");

        // Controls
        private DataGridView dataGridViewPatients = null!;
        private TextBox textBoxSearch = null!;
        private ComboBox comboBoxFilterDiagnosis = null!;
        private ComboBox comboBoxSort = null!;
        private ListView listViewSummary = null!;
        private Panel chartSummary = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        // Left control buttons
        private Button btnAdd = null!;
        private Button btnEdit = null!;
        private Button btnDelete = null!;
        private Button btnRefresh = null!;

        // Histogram cache
        private Dictionary<string, int> _lastHistogram = new Dictionary<string, int>();

        public FormMain()
        {
            Text = "Поликлиника - Викол А.С. ИСПб-25-1";
            Width = 1100;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            InitializeComponentCustom();

            Load += FormMain_Load;
            Resize += (s, e) => { dataGridViewPatients.AutoResizeColumns(); chartSummary.Invalidate(); };
        }

        private void InitializeComponentCustom()
        {
            Controls.Clear();

            // Top toolbar with two dropdowns
            var topTool = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden, BackColor = SystemColors.Control };
            var fileDrop = new ToolStripDropDownButton("Файл") { ShowDropDownArrow = true };
            fileDrop.DropDownItems.Add(new ToolStripMenuItem("Открыть", null, (s, e) => LoadData()));
            fileDrop.DropDownItems.Add(new ToolStripMenuItem("Сохранить", null, (s, e) => SaveData()));
            fileDrop.DropDownItems.Add(new ToolStripSeparator());
            fileDrop.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (s, e) => Close()));

            var helpDrop = new ToolStripDropDownButton("Справка") { ShowDropDownArrow = true };
            helpDrop.DropDownItems.Add(new ToolStripMenuItem("О программе", null, (s, e) =>
            {
                var about = new StringBuilder();
                about.AppendLine("Поликлиника — учебный проект");
                about.AppendLine();
                about.AppendLine("Назначение: учебное приложение для учёта пациентов — добавление, редактирование, удаление, фильтрация и анализ записей.");
                about.AppendLine();
                about.AppendLine("Основные данные:");
                about.AppendLine("  • Id (номер записи)");
                about.AppendLine("  • Фамилия, имя, отчество, дата рождения пациента");
                about.AppendLine("  • ФИО, должность и специализация лечащего врача");
                about.AppendLine("  • Диагноз, амбулаторное лечение, срок нетрудоспособности (дни), диспансерный учёт, примечание");
                about.AppendLine();
                about.AppendLine("Функциональность:");
                about.AppendLine("  • Добавление/редактирование/удаление записей (диалог редактирования)");
                about.AppendLine("  • Хранение данных в CSV (patients.csv) через DataService");
                about.AppendLine("  • Поиск по фамилии, сортировка, фильтр по диагнозу");
                about.AppendLine("  • Быстрая статистика в строке состояния и гистограмма распределения по диагнозам");
                about.AppendLine();
                about.AppendLine("Технологии: C#, .NET 8, WinForms.");
                about.AppendLine();
                about.AppendLine("Автор: Викол Александр (ИСПб-25-1). Проект учебный — можно модифицировать и использовать для практики.");
                MessageBox.Show(about.ToString(), "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));

            helpDrop.DropDownItems.Add(new ToolStripMenuItem("Краткое руководство", null, (s, e) =>
            {
                var guide = new StringBuilder();
                guide.AppendLine("Краткое руководство по работе с приложением:");
                guide.AppendLine();
                guide.AppendLine("1. Добавить пациента: нажмите 'Добавить пациента' и заполните форму, затем сохраните.");
                guide.AppendLine("2. Изменить пациента: выберите строку в таблице и нажмите 'Изменить пациента'.");
                guide.AppendLine("3. Удалить пациента: выберите строку и нажмите 'Удалить пациента'.");
                guide.AppendLine("4. Поиск: введите фамилию в поле 'Поиск (фамилия)' слева.");
                guide.AppendLine("5. Сортировка/Фильтр: используйте выпадающие списки слева для сортировки и фильтрации по диагнозу.");
                guide.AppendLine("6. Сохранение/Загрузка: используйте меню 'Файл' для работы с CSV (patients.csv).");
                guide.AppendLine("7. Статистика: правый блок показывает сводку по диагнозам и гистограмму.");
                MessageBox.Show(guide.ToString(), "Краткое руководство", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));

            topTool.Items.Add(fileDrop);
            topTool.Items.Add(helpDrop);
            Controls.Add(topTool);

            // Main layout
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8, 18, 8, 8) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            Controls.Add(mainLayout);

            // Left panel
            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            mainLayout.Controls.Add(leftPanel, 0, 0);
            var leftFlow = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
            leftPanel.Controls.Add(leftFlow);

            btnAdd = new Button { Text = "Добавить пациента", Width = 220, Height = 36, Margin = new Padding(2) };
            btnEdit = new Button { Text = "Изменить пациента", Width = 220, Height = 36, Margin = new Padding(2) };
            btnDelete = new Button { Text = "Удалить пациента", Width = 220, Height = 36, Margin = new Padding(2) };
            btnRefresh = new Button { Text = "Обновить", Width = 220, Height = 36, Margin = new Padding(2) };
            btnAdd.Click += (s, e) => { ShowEditDialog(null); RefreshControlsAfterLoad(); };
            btnEdit.Click += (s, e) => { if (dataGridViewPatients.SelectedRows.Count > 0) ShowEditDialog(GetSelectedPatient()); };
            btnDelete.Click += (s, e) => { if (dataGridViewPatients.SelectedRows.Count > 0) { try { _dataService.DeletePatient(GetSelectedPatient().Id); RefreshControlsAfterLoad(); } catch { } } };
            btnRefresh.Click += (s, e) => { RefreshControlsAfterLoad(); };

            leftFlow.Controls.Add(btnAdd);
            leftFlow.Controls.Add(btnEdit);
            leftFlow.Controls.Add(btnDelete);
            leftFlow.Controls.Add(btnRefresh);
            leftFlow.Controls.Add(new Label { Text = "", Height = 6 });

            leftFlow.Controls.Add(new Label { Text = "Поиск (фамилия):", AutoSize = false, Width = 240 });
            textBoxSearch = new TextBox { Width = 240 };
            textBoxSearch.TextChanged += (s, e) => RefreshGrid();
            leftFlow.Controls.Add(textBoxSearch);

            leftFlow.Controls.Add(new Label { Text = "Сортировка:", AutoSize = false, Width = 240 });
            comboBoxSort = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxSort.Items.AddRange(new string[] { "Общее", "ID", "Фамилия", "Возраст" }); comboBoxSort.SelectedIndex = 0;
            comboBoxSort.SelectedIndexChanged += (s, e) => RefreshGrid();
            leftFlow.Controls.Add(comboBoxSort);

            leftFlow.Controls.Add(new Label { Text = "Фильтр по диагнозу:", AutoSize = false, Width = 240 });
            comboBoxFilterDiagnosis = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDown };
            comboBoxFilterDiagnosis.TextChanged += (s, e) => RefreshGrid(); comboBoxFilterDiagnosis.SelectedIndexChanged += (s, e) => RefreshGrid();
            leftFlow.Controls.Add(comboBoxFilterDiagnosis);

            // Middle: grid
            dataGridViewPatients = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false, AllowUserToAddRows = false };
            dataGridViewPatients.RowTemplate.Height = 36;
            dataGridViewPatients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPatients.BackgroundColor = Color.White;
            dataGridViewPatients.GridColor = Color.LightGray;
            dataGridViewPatients.EnableHeadersVisualStyles = false;
            dataGridViewPatients.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = SystemColors.ControlLight, Font = new Font(Font.FontFamily, 10F, FontStyle.Bold) };
            dataGridViewPatients.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font(Font.FontFamily, 10F) };

            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = "FirstName", Name = "colFirstName" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = "LastName", Name = "colLastName" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = "MiddleName", Name = "colMiddleName" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата р.", DataPropertyName = "BirthDate", Name = "colBirthDate" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Врач", DataPropertyName = "DoctorFullName", Name = "colDoctor" });
            dataGridViewPatients.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "ID", Width = 60, Name = "colId" });

            mainLayout.Controls.Add(dataGridViewPatients, 1, 0);

            // Right: summary + chart
            var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            mainLayout.Controls.Add(rightPanel, 2, 0);

            var rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            rightPanel.Controls.Add(rightLayout);

            var summaryPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            summaryPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            summaryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightLayout.Controls.Add(summaryPanel, 0, 0);

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            var lblDiag = new Label { Text = "Диагноз", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font(Font.FontFamily, 9F, FontStyle.Bold) };
            var lblCount = new Label { Text = "Кол-во", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font(Font.FontFamily, 9F, FontStyle.Bold) };
            header.Controls.Add(lblDiag, 0, 0);
            header.Controls.Add(lblCount, 1, 0);
            summaryPanel.Controls.Add(header, 0, 0);

            listViewSummary = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HeaderStyle = ColumnHeaderStyle.None, Font = new Font(Font.FontFamily, 9F) };
            listViewSummary.Columns.Add("Diag", 200);
            listViewSummary.Columns.Add("Count", 80);
            summaryPanel.Controls.Add(listViewSummary, 0, 1);

            chartSummary = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };
            chartSummary.Paint += PanelSummary_Paint;
            rightLayout.Controls.Add(chartSummary, 0, 1);

            // Status strip
            statusStrip = new StatusStrip(); statusLabel = new ToolStripStatusLabel("Готов"); statusStrip.Items.Add(statusLabel); Controls.Add(statusStrip);
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            try { LoadData(); } catch { }
        }

        private void LoadData()
        {
            try
            {
                _dataService.LoadFromCsv(_dataPath);
                RefreshControlsAfterLoad();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
        }

        private void SaveData()
        {
            try { _dataService.SaveToCsv(_dataPath); statusLabel.Text = "Данные сохранены"; } catch (Exception ex) { MessageBox.Show("Ошибка сохранения: " + ex.Message); }
        }

        private void RefreshControlsAfterLoad()
        {
            var diagnoses = _data_service_safe_enumerable().Select(x => x.Diagnosis).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            comboBoxFilterDiagnosis.Items.Clear(); comboBoxFilterDiagnosis.Items.Add("(Все)"); comboBoxFilterDiagnosis.Items.AddRange(diagnoses); comboBoxFilterDiagnosis.SelectedIndex = 0;
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            var list = _data_service_safe_enumerable();
            if (!string.IsNullOrWhiteSpace(textBoxSearch.Text)) list = list.Where(x => x.LastName.IndexOf(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            var sel = comboBoxFilterDiagnosis.SelectedItem as string ?? comboBoxFilterDiagnosis.Text;
            if (!string.IsNullOrWhiteSpace(sel) && sel != "(Все)") list = list.Where(x => string.Equals(x.Diagnosis, sel, StringComparison.OrdinalIgnoreCase));
            var key = comboBoxSort.SelectedItem?.ToString() ?? "Общее";
            list = key switch { "Фамилия" => list.OrderBy(x => x.LastName), "Возраст" => list.OrderBy(x => x.Age), "Id" => list.OrderBy(x => x.Id), _ => list };

            var rows = list.Select(x => new
            {
                x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                BirthDate = x.BirthDate.ToShortDateString(),
                DoctorFullName = x.DoctorFullName,
                Diagnosis = x.Diagnosis
            }).ToList();

            dataGridViewPatients.DataSource = null;
            dataGridViewPatients.DataSource = rows;
            dataGridViewPatients.ClearSelection();

            UpdateSummary();
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            try
            {
                var stats = _data_service_safe_stats();
                statusLabel.Text = $"Пациентов: {stats.Count} | Средний возраст: {Math.Round(stats.AverageAge,1)} | Мин возраст / Макс возраст: {stats.MinAge}/{stats.MaxAge}";
            }
            catch { statusLabel.Text = "Пациентов: 0"; }
        }

        private void UpdateSummary()
        {
            _lastHistogram = _dataService.GetHistogramByDiagnosis();
            listViewSummary.BeginUpdate(); listViewSummary.Items.Clear();
            foreach (var kv in _lastHistogram.OrderByDescending(k => k.Value))
            {
                var item = new ListViewItem(new string[] { kv.Key, kv.Value.ToString() });
                listViewSummary.Items.Add(item);
            }
            listViewSummary.EndUpdate();
            chartSummary.Invalidate();
        }

        private void PanelSummary_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(chartSummary.BackColor);
            if (_lastHistogram == null || _lastHistogram.Count == 0)
            {
                using var f = new Font("Segoe UI", 10);
                g.DrawString("Нет данных", f, Brushes.Gray, 8, 8);
                return;
            }

            int margin = 12;
            int reservedTop = margin + 8; // space for value labels
            int reservedBottom = margin + 28; // space for category labels
            int w = Math.Max(0, chartSummary.ClientSize.Width - margin * 2);
            int h = Math.Max(0, chartSummary.ClientSize.Height - reservedTop - reservedBottom);

            var items = _lastHistogram.OrderByDescending(k => k.Value).ToList();
            int n = items.Count;
            int max = items.Max(i => i.Value);
            if (n == 0 || max == 0) return;

            int barWidth = Math.Max(14, w / (n * 2));
            int spacing = Math.Max(6, barWidth / 2);
            int x = margin;

            using var font = new Font("Segoe UI", 9);
            for (int i = 0; i < items.Count; i++)
            {
                var kv = items[i];
                int val = kv.Value;
                int barH = (int)(h * ((double)val / max));
                var rect = new Rectangle(x, reservedTop + (h - barH), barWidth, barH);

                using (var brush = new SolidBrush(Color.FromArgb(160, 100, 149, 237)))
                {
                    g.FillRectangle(brush, rect);
                }
                g.DrawRectangle(Pens.DarkGray, rect);

                // category label below
                var labelRect = new Rectangle(x - 6, reservedTop + h + 4, barWidth + 12, 36);
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(kv.Key, font, Brushes.Black, labelRect, sf);

                // value label above bar; ensure within visible area
                var sval = val.ToString();
                var size = g.MeasureString(sval, font);
                float valY = rect.Top - size.Height - 4;
                if (valY < 2) valY = rect.Top + 2; // if too tight, draw inside bar
                g.DrawString(sval, font, Brushes.Black, x + barWidth / 2 - size.Width / 2, valY);

                x += barWidth + spacing;
            }
        }

        private Patient GetSelectedPatient()
        {
            try
            {
                if (dataGridViewPatients.SelectedRows.Count > 0)
                {
                    var bound = dataGridViewPatients.SelectedRows[0].DataBoundItem;
                    if (bound != null)
                    {
                        var idProp = bound.GetType().GetProperty("Id");
                        if (idProp != null)
                        {
                            var idVal = idProp.GetValue(bound);
                            if (idVal is int iid) return _dataService.Patients.First(x => x.Id == iid);
                            if (idVal != null && int.TryParse(idVal.ToString(), out var parsed)) return _dataService.Patients.First(x => x.Id == parsed);
                        }
                    }

                    var idCell = dataGridViewPatients.SelectedRows[0].Cells.Cast<DataGridViewCell>().FirstOrDefault(c => string.Equals(c.OwningColumn.Name, "colId", StringComparison.OrdinalIgnoreCase));
                    if (idCell != null && idCell.Value != null && int.TryParse(idCell.Value.ToString(), out var id2)) return _dataService.Patients.First(x => x.Id == id2);
                }
            }
            catch { }

            return new Patient();
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
                if (isNew) _dataService.AddPatient(edit.Patient); else _dataService.UpdatePatient(edit.Patient);
                RefreshControlsAfterLoad();
            }
        }

        // helpers
        private (int Count, double AverageAge, int MinAge, int MaxAge) _data_service_safe_stats()
        {
            try { return _dataService.GetStatistics(); } catch { return (0, 0, 0, 0); }
        }

        private IEnumerable<Lib.Patient> _data_service_safe_enumerable()
        {
            try { return _dataService.Patients.AsEnumerable(); } catch { return Enumerable.Empty<Lib.Patient>(); }
        }
    }
}
