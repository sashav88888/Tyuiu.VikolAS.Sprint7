using System;
using System.Drawing;
using System.Windows.Forms;
using Tyuiu.VikolAS.Sprint7.Project.V6.Lib;

namespace Tyuiu.VikolAS.Sprint7.Project.V6
{
    // Простая форма для редактирования/создания пациента
    // диалоговое окно, в котором пользователь вводит данные пациента.
    
    // При нажатии ОК будут выполнены простые проверки и данные скопируются в объект `Patient`.
    public class FormEditPatientWindow_VikolAS : Form

    {
        // Объект Patient, который редактируется в этом диалоге.
        // При создании формы мы передаём копию или пустой объект, а по подтверждению (OK)
        // содержимое полей копируется обратно в этот объект.
        public Patient Patient { get; private set; }

        // Поля формы: текстовые поля, чекбоксы и т.д.
        
        private TextBox textLastName = null!; // ввод фамилии
        private TextBox textFirstName = null!; // ввод имени
        private TextBox textMiddleName = null!; // ввод отчества
        private DateTimePicker dateBirth = null!; // выбор даты рождения
        private TextBox textDoctor = null!; // ФИО врача
        private TextBox textPosition = null!; // должность врача
        private TextBox textDiagnosis = null!; // диагноз
        private CheckBox chkAmbulatory = null!; // амбулаторно?
        private NumericUpDown numSickDays = null!; // срок нетрудоспособности (дни)
        private CheckBox chkDisp = null!; // диспансерный учёт?
        private TextBox textNote = null!; // примечание
        private ToolTip toolTip = null!; // подсказки для полей

        // Конструктор: принимает объект Patient (можно пустой для создания новой записи)
        // Устанавливает заголовок и размер окна, вызывает создание UI и подписывается на событие Load.
        public FormEditPatientWindow_VikolAS(Patient p)
        {
            Patient = p;
            Text = p.Id == 0 ? "Добавление пациента - Tyuiu_VikolAS" : $"Изменение пациента {p.Id} - Tyuiu_VikolAS";
            Width = 560;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            CreateUi();
            Load += FormEditPatient_Load; // при загрузке заполним поля значениями из Patient
        }

        // Создаём все контролы формы и располагаем их в простой таблице
        
        private void CreateUi()
        {
            // Общие настройки внешнего вида
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.WhiteSmoke;

            toolTip = new ToolTip();
            toolTip.IsBalloon = true;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            Controls.Add(panel);

            // Используем TableLayoutPanel для простого выравнивания меток и полей
            var flow = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            flow.Padding = new Padding(6);
            flow.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            panel.Controls.Add(flow);

            int r = 0;
            flow.RowStyles.Clear();

            // Вспомогательная локальная функция для добавления одной строки (метка + контрол)
            void AddRow(Control lbl, Control ctrl)
            {
                flow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                flow.Controls.Add(lbl, 0, r);
                flow.Controls.Add(ctrl, 1, r);
                r++;
            }

            // Для каждого поля создаём метку и контрол. 
            var lblLast = new Label { Text = "Фамилия:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            textLastName = new TextBox { Width = 380, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblLast, textLastName);
            toolTip.SetToolTip(textLastName, "Введите фамилию пациента");

            var lblFirst = new Label { Text = "Имя:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            textFirstName = new TextBox { Width = 380, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblFirst, textFirstName);
            toolTip.SetToolTip(textFirstName, "Введите имя пациента");

            var lblMiddle = new Label { Text = "Отчество:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            textMiddleName = new TextBox { Width = 380, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblMiddle, textMiddleName);
            toolTip.SetToolTip(textMiddleName, "Введите отчество (если есть)");

            var lblBirth = new Label { Text = "Дата рождения:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            dateBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 200, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblBirth, dateBirth);
            toolTip.SetToolTip(dateBirth, "Выберите дату рождения пациента");

            var lblDoctor = new Label { Text = "Врач (ФИО):", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            textDoctor = new TextBox { Width = 380, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblDoctor, textDoctor);
            toolTip.SetToolTip(textDoctor, "ФИО лечащего врача");

            var lblPosition = new Label { Text = "Должность/спец.:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            textPosition = new TextBox { Width = 380, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblPosition, textPosition);
            toolTip.SetToolTip(textPosition, "Должность или специализация врача");

            var lblDiagnosis = new Label { Text = "Диагноз:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            textDiagnosis = new TextBox { Width = 380, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblDiagnosis, textDiagnosis);
            toolTip.SetToolTip(textDiagnosis, "Краткий диагноз пациента");

            var lblAmb = new Label { Text = "Амбулаторно:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            chkAmbulatory = new CheckBox { Margin = new Padding(6) };
            AddRow(lblAmb, chkAmbulatory);
            toolTip.SetToolTip(chkAmbulatory, "Отметьте, если лечение амбулаторное");

            var lblDays = new Label { Text = "Срок нетрудоспособности (дн):", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            numSickDays = new NumericUpDown { Minimum = 0, Maximum = 3650, Width = 100, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6) };
            AddRow(lblDays, numSickDays);
            toolTip.SetToolTip(numSickDays, "Укажите срок в днях");

            var lblDisp = new Label { Text = "На диспансерном учете:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            chkDisp = new CheckBox { Margin = new Padding(6) };
            AddRow(lblDisp, chkDisp);
            toolTip.SetToolTip(chkDisp, "Отметьте, если пациент на диспансерном учёте");

            var lblNote = new Label { Text = "Примечание:", AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            textNote = new TextBox { Width = 380, Height = 120, Multiline = true, Font = new Font(this.Font.FontFamily, 11F), Margin = new Padding(6), ScrollBars = ScrollBars.Vertical };
            AddRow(lblNote, textNote);
            toolTip.SetToolTip(textNote, "Дополнительная информация о пациенте");

            // Кнопки внизу: ОК и Отмена. ОК валидирует данные и закрывает форму с результатом OK.
            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12), Height = 64, BackColor = Color.Transparent };
            var btnOk = new Button { Text = "ОК", DialogResult = DialogResult.OK, Width = 140, Height = 40 };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Width = 140, Height = 40 };

            // Стиль кнопок 
            btnOk.Font = new Font(this.Font.FontFamily, 11F, FontStyle.Bold);
            btnOk.BackColor = Color.FromArgb(46, 139, 87);
            btnOk.ForeColor = Color.White;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.FlatAppearance.BorderSize = 0;

            btnCancel.Font = new Font(this.Font.FontFamily, 11F, FontStyle.Regular);
            btnCancel.BackColor = Color.LightGray;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;

            // Обработчик ОК — выполняет валидацию и копирует введённые значения в объект Patient
            btnOk.Click += BtnOk_Click;
            panelButtons.Controls.Add(btnOk);
            panelButtons.Controls.Add(btnCancel);
            Controls.Add(panelButtons);

            // Удобства: Enter = OK, Esc = Cancel
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        // При загрузке формы копируем значения из объекта Patient в контролы
        private void FormEditPatient_Load(object? sender, EventArgs e)
        {
            // Если Patient пустой (новая запись) — устанавливаем разумные значения по умолчанию
            textLastName.Text = Patient.LastName;
            textFirstName.Text = Patient.FirstName;
            textMiddleName.Text = Patient.MiddleName;
            dateBirth.Value = Patient.BirthDate == DateTime.MinValue ? DateTime.Today.AddYears(-30) : Patient.BirthDate;
            textDoctor.Text = Patient.DoctorFullName;
            textPosition.Text = Patient.DoctorPosition;
            textDiagnosis.Text = Patient.Diagnosis;
            chkAmbulatory.Checked = Patient.Ambulatory;
            numSickDays.Value = Patient.SickLeaveDays;
            chkDisp.Checked = Patient.OnDispensary;
            textNote.Text = Patient.Note;
        }

        /*
         * BtnOk_Click
         * Простая валидация: проверяем, что заполнены имя и фамилия.
         * Если всё ок — копируем данные из контролов в объект Patient и форма закроется с DialogResult.OK.
         */
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            // Проверка обязательных полей — фамилия и имя
            if (string.IsNullOrWhiteSpace(textLastName.Text) || string.IsNullOrWhiteSpace(textFirstName.Text))
            {
                MessageBox.Show("Заполните имя и фамилию", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None; // отменяем закрытие формы с OK
                return;
            }

            // Копируем все значения в объект Patient
            Patient.LastName = textLastName.Text.Trim();
            Patient.FirstName = textFirstName.Text.Trim();
            Patient.MiddleName = textMiddleName.Text.Trim();
            Patient.BirthDate = dateBirth.Value.Date;
            Patient.DoctorFullName = textDoctor.Text.Trim();
            Patient.DoctorPosition = textPosition.Text.Trim();
            Patient.Diagnosis = textDiagnosis.Text.Trim();
            Patient.Ambulatory = chkAmbulatory.Checked;
            Patient.SickLeaveDays = (int)numSickDays.Value;
            Patient.OnDispensary = chkDisp.Checked;
            Patient.Note = textNote.Text.Trim();

            // После этого форма закроется с DialogResult.OK (установлено в кнопке)
        }
    }

    // Обёртка: оставляем имя FormEditPatient чтобы остальной код проекта работал без изменений
    public class FormEditPatient : FormEditPatientWindow_VikolAS
    {
        public FormEditPatient(Patient p) : base(p)
        {
        }
    }
}
