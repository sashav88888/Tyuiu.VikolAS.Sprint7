using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Tyuiu.VikolAS.Sprint7.Project.V6.Lib
{
    /*
     * DataService.cs
     * Упрощённый сервис для хранения списка пациентов и работы с простым CSV-файлом.
     * Этот файл содержит два класса:
     *  - Patient: структура (класс) для хранения данных про одного пациента.
     *  - DataService: набор простых методов для работы со списком пациентов (CRUD, загрузка/сохранение, базовая статистика).
     *
     * Цель упрощения: код должен быть понятен студенту 1 курса.
     * Формат записи в файл: поля разделяются символом '|' (вертикальная черта).
     * Пример строки в файле:
     *  1|Иванов|Иван|И.|2000-01-01T00:00:00.0000000Z|Др. Смирнов|Терапевт|ОРВИ|1|0|0|Примечание
     *
     * Замечания:
     *  - Такой простой формат удобен для учебного проекта, но не подходит для сложных реальных данных
     *    (если в полях могут встречаться разделители или переносы строк).
     */

    // Модель данных для одного пациента.
    public class Patient
    {
        // Id записи (в примерах создаётся автоматически при добавлении).
        public int Id { get; set; }

        // Фамилия пациента
        public string LastName { get; set; } = string.Empty;

        // Имя пациента
        public string FirstName { get; set; } = string.Empty;

        // Отчество пациента (если есть)
        public string MiddleName { get; set; } = string.Empty;

        // Дата рождения — используем DateTime, хранится и сериализуется в формате round-trip (o)
        public DateTime BirthDate { get; set; }

        // ФИО лечащего врача
        public string DoctorFullName { get; set; } = string.Empty;

        // Должность или специализация врача
        public string DoctorPosition { get; set; } = string.Empty;

        // Диагноз пациента (строка)
        public string Diagnosis { get; set; } = string.Empty;

        // Признак амбулаторного лечения; в файле сохраняется как "1" или "0"
        public bool Ambulatory { get; set; }

        // Количество дней нетрудоспособности
        public int SickLeaveDays { get; set; }

        // На диспансерном учёте (true/false)
        public bool OnDispensary { get; set; }

        // Дополнительная заметка
        public string Note { get; set; } = string.Empty;

        // Упрощённое свойство: возраст в полных годах
        public int Age => (int)((DateTime.Today - BirthDate).TotalDays / 365.2425);

        /*
         * ToCsv
         * Собирает все важные поля в одну строку для записи в файл.
         * Мы используем разделитель '|' и предварительно очищаем текстовые поля от переносов строк
         * и самого разделителя, чтобы не ломать формат. Это простая, но понятная реализация.
         */
        public string ToCsv()
        {
            // Простая функция очистки текстовых полей: убирает переносы строк и символ '|' если он есть
            string clean(string s) => string.IsNullOrEmpty(s) ? string.Empty : s.Replace("\r", " ").Replace("\n", " ").Replace("|", " ");

            var parts = new string[]
            {
                Id.ToString(),
                clean(LastName),
                clean(FirstName),
                clean(MiddleName),
                BirthDate.ToString("o", CultureInfo.InvariantCulture), // сохраняем дату в нормальном формате
                clean(DoctorFullName),
                clean(DoctorPosition),
                clean(Diagnosis),
                Ambulatory ? "1" : "0",
                SickLeaveDays.ToString(),
                OnDispensary ? "1" : "0",
                clean(Note)
            };

            // Соединяем все части вертикальной чертой и возвращаем
            return string.Join("|", parts);
        }

        /*
         * FromCsv
         * Простой разбор строки из файла обратно в объект Patient.
         * - Разделитель: '|'
         * - Если каких-то полей не хватает или они некорректны — используем значения по умолчанию.
         */
        public static Patient FromCsv(string line)
        {
            var p = new Patient();
            if (string.IsNullOrWhiteSpace(line)) return p; // если строка пустая — вернуть пустой объект

            // Разбиваем по разделителю. Для учебного примера этого достаточно.
            var parts = line.Split('|');
            try
            {
                int idx = 0;
                if (idx < parts.Length && int.TryParse(parts[idx++], out var id)) p.Id = id;
                if (idx < parts.Length) p.LastName = parts[idx++];
                if (idx < parts.Length) p.FirstName = parts[idx++];
                if (idx < parts.Length) p.MiddleName = parts[idx++];
                if (idx < parts.Length && DateTime.TryParse(parts[idx++], null, DateTimeStyles.RoundtripKind, out var bd)) p.BirthDate = bd;
                if (idx < parts.Length) p.DoctorFullName = parts[idx++];
                if (idx < parts.Length) p.DoctorPosition = parts[idx++];
                if (idx < parts.Length) p.Diagnosis = parts[idx++];
                if (idx < parts.Length) p.Ambulatory = parts[idx++] == "1";
                if (idx < parts.Length && int.TryParse(parts[idx++], out var days)) p.SickLeaveDays = days;
                if (idx < parts.Length) p.OnDispensary = parts[idx++] == "1";
                if (idx < parts.Length) p.Note = parts[idx++];
            }
            catch
            {
                // В учебном проекте пропускаем ошибки. В реальном приложении — нужно логировать или выбрасывать исключение.
            }

            return p;
        }
    }

    /*
     * Простая служба данных: хранит список пациентов в памяти и умеет читать/писать в файл.
     * Методы названы простым и понятным образом — AddPatient, UpdatePatient, DeletePatient, LoadFromCsv, SaveToCsv.
     */
    public class DataService
    {
        // В памяти — простой список пациентов. 
        public List<Patient> Patients { get; } = new List<Patient>();

        /*
         * LoadFromCsv
         *  - очищаем текущий список
         *  - читаем все строки из файла
         *  - если первая строка выглядит как заголовок (начинается с "Id|"), пропускаем её
         *  - парсим каждую строку через Patient.FromCsv
         */
        public void LoadFromCsv(string path)
        {
            Patients.Clear();
            if (!File.Exists(path)) return; // файла нет — оставляем пустой список

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0) return; // файл пустой

            int start = 0;
            if (lines[0].StartsWith("Id|", StringComparison.OrdinalIgnoreCase)) start = 1; // если есть заголовок

            for (int i = start; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue; // пропускаем пустые строки
                var p = Patient.FromCsv(line);
                Patients.Add(p);
            }
        }

        /*
         * SaveToCsv
         *  - формируем заголовок (удобно для человека)
         *  - записываем каждую запись через Patient.ToCsv
         */
        public void SaveToCsv(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id|LastName|FirstName|MiddleName|BirthDate|DoctorFullName|DoctorPosition|Diagnosis|Ambulatory|SickLeaveDays|OnDispensary|Note");
            foreach (var p in Patients)
            {
                sb.AppendLine(p.ToCsv());
            }
            File.WriteAllText(path, sb.ToString());
        }

        // AddPatient: добавляет пациента в список, если Id не задан — генерирует новый простой Id
        public void AddPatient(Patient p)
        {
            if (p.Id == 0) p.Id = Patients.Count == 0 ? 1 : Patients.Max(x => x.Id) + 1;
            Patients.Add(p);
        }

        // UpdatePatient: находит запись по Id и копирует поля
        public void UpdatePatient(Patient p)
        {
            var existing = Patients.FirstOrDefault(x => x.Id == p.Id);
            if (existing == null) return; // если не найдено — ничего не делаем

            existing.LastName = p.LastName;
            existing.FirstName = p.FirstName;
            existing.MiddleName = p.MiddleName;
            existing.BirthDate = p.BirthDate;
            existing.DoctorFullName = p.DoctorFullName;
            existing.DoctorPosition = p.DoctorPosition;
            existing.Diagnosis = p.Diagnosis;
            existing.Ambulatory = p.Ambulatory;
            existing.SickLeaveDays = p.SickLeaveDays;
            existing.OnDispensary = p.OnDispensary;
            existing.Note = p.Note;
        }

        // DeletePatient: удаляет пациента по Id
        public void DeletePatient(int id)
        {
            var ex = Patients.FirstOrDefault(x => x.Id == id);
            if (ex != null) Patients.Remove(ex);
        }

        // GetStatistics: возвращает количество записей и простые возрастные метрики
        public (int Count, double AverageAge, int MinAge, int MaxAge) GetStatistics()
        {
            if (!Patients.Any()) return (0, 0, 0, 0);
            var ages = Patients.Select(x => x.Age).ToList();
            return (ages.Count, ages.Average(), ages.Min(), ages.Max());
        }

        // GetHistogramByDiagnosis: группирует пациентов по диагнозу и возвращает словарь (диагноз -> количество)
        public Dictionary<string, int> GetHistogramByDiagnosis()
        {
            return Patients.GroupBy(x => string.IsNullOrWhiteSpace(x.Diagnosis) ? "(нет)" : x.Diagnosis)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
