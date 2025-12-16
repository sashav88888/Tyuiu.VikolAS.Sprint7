using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Tyuiu.VikolAS.Sprint7.Project.V6.Lib
{
    // Простая модель пациента и сервис для работы с CSV.
    // Этот файл содержит две основные сущности:
    // 1) `Patient` - модель данных пациента с набором свойств,
    //    методами сериализации в CSV и десериализации из CSV.
    // 2) `DataService` - сервис для работы с коллекцией пациентов,
    //    загрузки/сохранения в CSV и простых операций (CRUD, фильтрация, статистика).

    public class Patient
    {
        // Уникальный идентификатор пациента
        public int Id { get; set; }

        // Фамилия пациента
        public string LastName { get; set; } = string.Empty; // Фамилия

        // Имя пациента
        public string FirstName { get; set; } = string.Empty; // Имя

        // Отчество пациента
        public string MiddleName { get; set; } = string.Empty; // Отчество

        // Дата рождения
        public DateTime BirthDate { get; set; } // Дата рождения

        // ФИО лечащего врача
        public string DoctorFullName { get; set; } = string.Empty; // Врач ФИО

        // Должность / специализация врача
        public string DoctorPosition { get; set; } = string.Empty; // Должность / специализация

        // Диагноз, поставленный пациенту
        public string Diagnosis { get; set; } = string.Empty; // Диагноз

        // Признак амбулаторного лечения
        public bool Ambulatory { get; set; } // Амбулаторно

        // Срок потери трудоспособности в днях
        public int SickLeaveDays { get; set; } // Срок потери трудоспособности

        // На диспансерном учёте
        public bool OnDispensary { get; set; } // Диспансерный учёт

        // Дополнительное примечание
        public string Note { get; set; } = string.Empty; // Примечание

        // Вычисляемое свойство: возраст в полных годах.
        // Используется простая формула на основе количества дней.
        public int Age => (int)((DateTime.Today - BirthDate).TotalDays / 365.2425);

        // Сериализация пациента в CSV-строку.
        // Поля экранируются кавычками, внутри кавычек двойные кавычки дублируются.
        public string ToCsv()
        {
            // Вспомогательная функция для экранирования строковых полей
            string esc(string s) => '"' + s.Replace("\"", "\"\"") + '"';
            // Собираем поля в массив строк и объединяем через запятую
            var fields = new string[]
            {
                Id.ToString(),
                esc(LastName),
                esc(FirstName),
                esc(MiddleName),
                esc(BirthDate.ToString("o", CultureInfo.InvariantCulture)),
                esc(DoctorFullName),
                esc(DoctorPosition),
                esc(Diagnosis),
                Ambulatory.ToString(),
                SickLeaveDays.ToString(),
                OnDispensary.ToString(),
                esc(Note)
            };
            return string.Join(",", fields);
        }

        // Десериализация пациента из CSV-строки.
        // Реализован простой парсер: поддерживает кавычки и двойные кавычки внутри полей.
        // Для учебного проекта этого достаточно, но в реальном приложении лучше использовать
        // полноценную CSV-библиотеку (например, CsvHelper).
        public static Patient FromCsv(string line)
        {
            // Разбиваем строку на поля, учитывая кавычки
            var parts = new List<string>();
            bool inQuotes = false;
            var cur = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Внутри кавычек встретилась пара кавычек - это символ ", добавляем его и пропускаем следующий символ
                        cur.Append('"');
                        i++; // пропускаем второй
                    }
                    else
                    {
                        // Переключаем режим внутри/вне кавычек
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    // Разделитель поля (запятая) вне кавычек
                    parts.Add(cur.ToString());
                    cur.Clear();
                }
                else
                {
                    cur.Append(ch);
                }
            }
            parts.Add(cur.ToString());

            // Создаём объект Patient и пытаемся заполнить поля
            var p = new Patient();
            try
            {
                int idx = 0;
                if (idx < parts.Count) p.Id = int.Parse(parts[idx++]);
                if (idx < parts.Count) p.LastName = TrimQuotes(parts[idx++]);
                if (idx < parts.Count) p.FirstName = TrimQuotes(parts[idx++]);
                if (idx < parts.Count) p.MiddleName = TrimQuotes(parts[idx++]);
                if (idx < parts.Count) p.BirthDate = DateTime.Parse(TrimQuotes(parts[idx++]), null, DateTimeStyles.RoundtripKind);
                if (idx < parts.Count) p.DoctorFullName = TrimQuotes(parts[idx++]);
                if (idx < parts.Count) p.DoctorPosition = TrimQuotes(parts[idx++]);
                if (idx < parts.Count) p.Diagnosis = TrimQuotes(parts[idx++]);
                if (idx < parts.Count) p.Ambulatory = bool.TryParse(parts[idx++], out var amb) ? amb : false;
                if (idx < parts.Count) p.SickLeaveDays = int.TryParse(parts[idx++], out var days) ? days : 0;
                if (idx < parts.Count) p.OnDispensary = bool.TryParse(parts[idx++], out var ondisp) ? ondisp : false;
                if (idx < parts.Count) p.Note = TrimQuotes(parts[idx++]);
            }
            catch
            {
                // Если при разборе произошла ошибка — возвращаем частично заполненный объект.
                // В вызывающем коде следует учитывать возможность некорректных данных.
            }
            return p;

            // Вспомогательная функция для удаления внешних кавычек и восстановления двойных кавычек
            static string TrimQuotes(string s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                    return s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
                return s;
            }
        }
    }

    // Сервис для управления коллекцией пациентов и сохранения/загрузки в CSV.
    // Этот сервис выполняет функцию «back-end» логики приложения: CRUD операции,
    // фильтрация, сортировка, статистика и формирование данных для визуализации.
    public class DataService
    {
        // Коллекция пациентов в памяти. В реальном приложении можно заменить на БД.
        public List<Patient> Patients { get; } = new List<Patient>();

        // Загрузить список пациентов из CSV-файла по указанному пути.
        // Если файл отсутствует — просто очистить текущую коллекцию.
        public void LoadFromCsv(string path)
        {
            Patients.Clear();
            if (!File.Exists(path)
                )
                return;

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0)
                return;

            // Если в файле есть заголовок — пропускаем первую строку
            int start = 0;
            if (lines[0].StartsWith("Id", StringComparison.OrdinalIgnoreCase))
                start = 1;

            for (int i = start; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                var p = Patient.FromCsv(line);
                Patients.Add(p);
            }
        }

        // Сохранить текущую коллекцию пациентов в CSV-файл.
        public void SaveToCsv(string path)
        {
            var sb = new StringBuilder();
            // Добавляем строку заголовка для удобства чтения/редактирования
            sb.AppendLine("Id,LastName,FirstName,MiddleName,BirthDate,DoctorFullName,DoctorPosition,Diagnosis,Ambulatory,SickLeaveDays,OnDispensary,Note");
            foreach (var p in Patients)
            {
                sb.AppendLine(p.ToCsv());
            }
            File.WriteAllText(path, sb.ToString());
        }

        // Добавить нового пациента в коллекцию.
        // Если Id не задан (0) — генерируем новый Id на основе максимального существующего.
        public void AddPatient(Patient p)
        {
            if (p.Id == 0)
                p.Id = Patients.Count == 0 ? 1 : Patients.Max(x => x.Id) + 1;
            Patients.Add(p);
        }

        // Обновить существующего пациента по Id — копируем поля из переданного объекта.
        public void UpdatePatient(Patient p)
        {
            var existing = Patients.FirstOrDefault(x => x.Id == p.Id);
            if (existing == null) return;
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

        // Удалить пациента по Id.
        public void DeletePatient(int id)
        {
            var existing = Patients.FirstOrDefault(x => x.Id == id);
            if (existing != null)
                Patients.Remove(existing);
        }

        // Поиск по фамилии (contains, регистронезависимо)
        public List<Patient> SearchByLastName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Patients.ToList();
            return Patients.Where(x => x.LastName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        // Сортировка по ключу (например: "Фамилия", "Возраст")
        public List<Patient> SortBy(string key, bool ascending = true)
        {
            IEnumerable<Patient> q = Patients;
            switch (key)
            {
                case "Фамилия": q = ascending ? q.OrderBy(x => x.LastName) : q.OrderByDescending(x => x.LastName); break;
                case "Возраст": q = ascending ? q.OrderBy(x => x.Age) : q.OrderByDescending(x => x.Age); break;
                default: q = q.OrderBy(x => x.Id); break;
            }
            return q.ToList();
        }

        // Фильтрация по точному совпадению диагноза (регистронезависимо)
        public List<Patient> FilterByDiagnosis(string diagnosis)
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
                return Patients.ToList();
            return Patients.Where(x => string.Equals(x.Diagnosis, diagnosis, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Простейшая статистика: количество, средний возраст, мин/макс возраста
        public (int Count, double AverageAge, int MinAge, int MaxAge) GetStatistics()
        {
            if (!Patients.Any())
                return (0, 0, 0, 0);
            var ages = Patients.Select(x => x.Age).ToList();
            return (ages.Count, ages.Average(), ages.Min(), ages.Max());
        }

        // Гистограмма по диагнозам: ключ — диагноз, значение — количество пациентов
        public Dictionary<string, int> GetHistogramByDiagnosis()
        {
            return Patients.GroupBy(x => string.IsNullOrWhiteSpace(x.Diagnosis) ? "(нет)" : x.Diagnosis)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }

}
