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
            ds.Patients.Clear();
            ds.Patients.Add(new Patient { LastName = "Иванов", FirstName = "Иван", MiddleName = "Иваныч", BirthDate = System.DateTime.Today.AddYears(-30), Diagnosis = "ОРВИ" });
            ds.Patients.Add(new Patient { LastName = "Петров", FirstName = "Пётр", MiddleName = "Петрович", BirthDate = System.DateTime.Today.AddYears(-40), Diagnosis = "Грипп" });

            var path = Path.Combine(Path.GetTempPath(), "test_patients.csv");
            if (File.Exists(path)) File.Delete(path);

            ds.SaveToCsv(path);
            var ds2 = new DataService();
            ds2.LoadFromCsv(path);

            Assert.AreEqual(2, ds2.Patients.Count);
            Assert.IsTrue(ds2.Patients.Any(p => p.LastName == "Иванов"));
            Assert.IsTrue(ds2.Patients.Any(p => p.LastName == "Петров"));

            File.Delete(path);
        }
    }
}
