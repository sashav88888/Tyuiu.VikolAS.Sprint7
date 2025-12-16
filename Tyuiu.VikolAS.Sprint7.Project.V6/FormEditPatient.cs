using System;
using System.Drawing;
using System.Windows.Forms;
using Tyuiu.VikolAS.Sprint7.Project.V6.Lib;

namespace Tyuiu.VikolAS.Sprint7.Project.V6
{
    // Простая форма для редактирования/создания пациента
    public class FormEditPatientWindow_VikolAS : Form
    {
        public Patient Patient { get; private set; }

        private TextBox textLastName = null!;
        private TextBox textFirstName = null!;
        private TextBox textMiddleName = null!;
        private DateTimePicker dateBirth = null!;
        private TextBox textDoctor = null!;
        private TextBox textPosition = null!;
        private TextBox textDiagnosis = null!;
        private CheckBox chkAmbulatory = null!;
        private NumericUpDown numSickDays = null!;
        private CheckBox chkDisp = null!;
        private TextBox textNote = null!;

        public FormEditPatientWindow_VikolAS(Patient p)
        {
            Patient = p;
            Text = p.Id == 0 ? "Добавление пациента - Tyuiu_VikolAS" : $"Изменение пациента {p.Id} - Tyuiu_VikolAS";
            Width = 480;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;

            CreateUi();
            Load += FormEditPatient_Load;
        }

        private void CreateUi()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            Controls.Add(panel);

            var flow = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 11, AutoSize = true };
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            panel.Controls.Add(flow);

            int r = 0;
            flow.RowStyles.Clear();
            for (int i = 0; i < 11; i++) flow.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            flow.Controls.Add(new Label { Text = "Фамилия:", AutoSize = true }, 0, r);
            textLastName = new TextBox { Width = 280 }; flow.Controls.Add(textLastName, 1, r++);

            flow.Controls.Add(new Label { Text = "Имя:", AutoSize = true }, 0, r);
            textFirstName = new TextBox { Width = 280 }; flow.Controls.Add(textFirstName, 1, r++);

            flow.Controls.Add(new Label { Text = "Отчество:", AutoSize = true }, 0, r);
            textMiddleName = new TextBox { Width = 280 }; flow.Controls.Add(textMiddleName, 1, r++);

            flow.Controls.Add(new Label { Text = "Дата рождения:", AutoSize = true }, 0, r);
            dateBirth = new DateTimePicker { Format = DateTimePickerFormat.Short }; flow.Controls.Add(dateBirth, 1, r++);

            flow.Controls.Add(new Label { Text = "Врач (ФИО):", AutoSize = true }, 0, r);
            textDoctor = new TextBox { Width = 280 }; flow.Controls.Add(textDoctor, 1, r++);

            flow.Controls.Add(new Label { Text = "Должность/спец.:", AutoSize = true }, 0, r);
            textPosition = new TextBox { Width = 280 }; flow.Controls.Add(textPosition, 1, r++);

            flow.Controls.Add(new Label { Text = "Диагноз:", AutoSize = true }, 0, r);
            textDiagnosis = new TextBox { Width = 280 }; flow.Controls.Add(textDiagnosis, 1, r++);

            flow.Controls.Add(new Label { Text = "Амбулаторно:", AutoSize = true }, 0, r);
            chkAmbulatory = new CheckBox(); flow.Controls.Add(chkAmbulatory, 1, r++);

            flow.Controls.Add(new Label { Text = "Срок нетрудоспособности (дн):", AutoSize = true }, 0, r);
            numSickDays = new NumericUpDown { Minimum = 0, Maximum = 3650, Width = 80 }; flow.Controls.Add(numSickDays, 1, r++);

            flow.Controls.Add(new Label { Text = "На диспансерном учете:", AutoSize = true }, 0, r);
            chkDisp = new CheckBox(); flow.Controls.Add(chkDisp, 1, r++);

            flow.Controls.Add(new Label { Text = "Примечание:", AutoSize = true }, 0, r);
            textNote = new TextBox { Width = 280, Height = 80, Multiline = true }; flow.Controls.Add(textNote, 1, r++);

            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8), Height = 48 };
            var btnOk = new Button { Text = "ОК", DialogResult = DialogResult.OK, Width = 100 };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Width = 100 };
            btnOk.Click += BtnOk_Click;
            panelButtons.Controls.Add(btnOk);
            panelButtons.Controls.Add(btnCancel);
            Controls.Add(panelButtons);
        }

        private void FormEditPatient_Load(object? sender, EventArgs e)
        {
            // Заполнить поля из Patient
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

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            // Простая валидация
            if (string.IsNullOrWhiteSpace(textLastName.Text) || string.IsNullOrWhiteSpace(textFirstName.Text))
            {
                MessageBox.Show("Заполните имя и фамилию");
                DialogResult = DialogResult.None;
                return;
            }

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
        }
    }

    // Wrapper class to preserve original type name expected by the rest of the codebase
    public class FormEditPatient : FormEditPatientWindow_VikolAS
    {
        public FormEditPatient(Patient p) : base(p)
        {
        }
    }
}
