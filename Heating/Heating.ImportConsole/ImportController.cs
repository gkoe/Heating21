using Heating.Core.Entities;

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Utils;

namespace Heating.ImportConsole
{
    public class ImportController
    {
        /// <summary>
        /// Liefert die Nutzungen mit den dazugehörigen Personen und Geräten
        /// zurück. Mehrfach vorkommende Personen und Geräte dürfen nur jeweils
        /// einmal existieren.
        /// </summary>
        /// <returns>Nutzungen aus der CSV-Datei</returns>
        public static async Task<Measurement[]> ReadFromCsvAsync()
        {
            string[][] matrix = await MyFile.ReadStringMatrixFromCsvAsync("measurements.csv", true);
            var sensors = matrix
                .GroupBy(l => l[0])
                .Select(grp => new Sensor { Name = grp.Key })
                .ToList();
            CultureInfo provider = CultureInfo.InvariantCulture;
            var measurements = matrix
                .Select(line => new Measurement
                {
                    Sensor = sensors.Single(s => s.Name == line[0]),
                    //Time = DateTime.Parse(line[1]),
                    Time = DateTime.ParseExact(line[1], "dd.MM.yyyy HH:mm:ss", provider),
                    Value = double.Parse(line[2])
                })
                .ToArray();
            return measurements;
        }

    }
}
