using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tyuiu.VikolAS.Sprint7.Project.V6.Lib;
using System.IO;
using System.Linq;

namespace Tyuiu.VikolAS.Sprint7.Project.V6.Test
{
    [TestClass]
    public sealed class DataServiceTest
    {
        [TestMethod]
        public void SaveLoadRoundtrip()
        {
            
            var ds = new DataService();
            ds.Patients.Clear(); // очищаем список пациентов перед тестом
            // Добавляем двух пациентов для теста
            ds.Patients.Add(new Patient { LastName = "Иванов", FirstName = "Иван", MiddleName = "Иваныч", BirthDate = System.DateTime.Today.AddYears(-30), Diagnosis = "ОРВИ" });
            ds.Patients.Add(new Patient { LastName = "Петров", FirstName = "Пётр", MiddleName = "Петрович", BirthDate = System.DateTime.Today.AddYears(-40), Diagnosis = "Грипп" });

            // Формируем путь к временному CSV-файлу
            var path = Path.Combine(Path.GetTempPath(), "test_patients.csv");
            if (File.Exists(path)) File.Delete(path); // удаляем файл если он случайно уже существует

            ds.SaveToCsv(path); // сохраняем данные в CSV
            var ds2 = new DataService(); // создаём новый сервис для загрузки
            ds2.LoadFromCsv(path); // загружаем данные из CSV

            // Проверяем что загрузилось ровно 2 записи и фамилии присутствуют
            Assert.AreEqual(2, ds2.Patients.Count);
            Assert.IsTrue(ds2.Patients.Any(p => p.LastName == "Иванов"));
            Assert.IsTrue(ds2.Patients.Any(p => p.LastName == "Петров"));

            File.Delete(path); // очищаем временный файл
        }
    }
}
