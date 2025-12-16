using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Tyuiu.VikolAS.Sprint7.Project.V6.Lib
{
    // Простенький модель пациента и сервис для работы с CSV
    public class Patient
    {
        public int Id { get; set; }
        public string LastName { get; set; } = string.Empty; // Фамилия
        public string FirstName { get; set; } = string.Empty; // Имя
        public string MiddleName { get; set; } = string.Empty; // Отчество
        public DateTime BirthDate { get; set; } // Дата рождения
        public string DoctorFullName { get; set; } = string.Empty; // Врач ФИО
        public string DoctorPosition { get; set; } = string.Empty; // Должность / специализация
        public string Diagnosis { get; set; } = string.Empty; // Диагноз
        public bool Ambulatory { get; set; } // Амбулаторно
        public int SickLeaveDays { get; set; } // Срок потери трудоспособности
        public bool OnDispensary { get; set; } // Диспансерный учёт
        public string Note { get; set; } = string.Empty; // Примечание

        // Возраст в полных годах
        public int Age => (int)((DateTime.Today - BirthDate).TotalDays / 365.2425);

        public string ToCsv()
        {
            // Простая CSV-строка, экранируем кавычки
            string esc(string s) => '"' + s.Replace("\"", "\"\"") + '"';
            // Собираем поля в строковый массив, затем объединяем через запятую
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

        public static Patient FromCsv(string line)
        {
            // Очень простой CSV-парсер, ожидаем, что поля в кавычках или без
            // Для учебного проекта этого достаточно
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
                        // двойная кавычка внутри строки
                        cur.Append('"');
                        i++; // пропускаем
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    parts.Add(cur.ToString());
                    cur.Clear();
                }
                else
                {
                    cur.Append(ch);
                }
            }
            parts.Add(cur.ToString());

            // Теперь создаём объект
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
                // Если что-то пошло не так — вернём пустого пациента, вызывающий код должен учитывать
            }
            return p;

            static string TrimQuotes(string s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                    return s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
                return s;
            }
        }
    }

    public class DataService
    {
        // Сервис для работы с коллекцией пациентов и CSV
        public List<Patient> Patients { get; } = new List<Patient>();

        // Загрузить из CSV
        public void LoadFromCsv(string path)
        {
            Patients.Clear();
            if (!File.Exists(path))
                return;

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0)
                return;

            // если есть заголовок — пропускаем его
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

        // Сохранить в CSV
        public void SaveToCsv(string path)
        {
            var sb = new StringBuilder();
            // заголовок
            sb.AppendLine("Id,LastName,FirstName,MiddleName,BirthDate,DoctorFullName,DoctorPosition,Diagnosis,Ambulatory,SickLeaveDays,OnDispensary,Note");
            foreach (var p in Patients)
            {
                sb.AppendLine(p.ToCsv());
            }
            File.WriteAllText(path, sb.ToString());
        }

        // Добавить пациента
        public void AddPatient(Patient p)
        {
            // простая логика для id
            if (p.Id == 0)
                p.Id = Patients.Count == 0 ? 1 : Patients.Max(x => x.Id) + 1;
            Patients.Add(p);
        }

        // Обновить пациента по id
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

        // Удалить
        public void DeletePatient(int id)
        {
            var existing = Patients.FirstOrDefault(x => x.Id == id);
            if (existing != null)
                Patients.Remove(existing);
        }

        // Поиск по фамилии (contains)
        public List<Patient> SearchByLastName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Patients.ToList();
            return Patients.Where(x => x.LastName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        // Сортировка по фамилии или возрасту
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

        // Фильтрация по диагнозу
        public List<Patient> FilterByDiagnosis(string diagnosis)
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
                return Patients.ToList();
            return Patients.Where(x => string.Equals(x.Diagnosis, diagnosis, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Статистика: count, average age, min/max age
        public (int Count, double AverageAge, int MinAge, int MaxAge) GetStatistics()
        {
            if (!Patients.Any())
                return (0, 0, 0, 0);
            var ages = Patients.Select(x => x.Age).ToList();
            return (ages.Count, ages.Average(), ages.Min(), ages.Max());
        }

        // Гистограмма по диагнозам
        public Dictionary<string, int> GetHistogramByDiagnosis()
        {
            return Patients.GroupBy(x => string.IsNullOrWhiteSpace(x.Diagnosis) ? "(нет)" : x.Diagnosis)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
