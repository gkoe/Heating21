using System;
using System.Linq;

using Heating.ImportConsole;
using Heating.Persistence;

Console.WriteLine("Import der Measurements und Sensors in die Datenbank");
await using UnitOfWork unitOfWork = new UnitOfWork();
Console.WriteLine("Datenbank löschen");
await unitOfWork.DeleteDatabaseAsync();
Console.WriteLine("Datenbank migrieren");
await unitOfWork.MigrateDatabaseAsync();
Console.WriteLine("Measurements und sensors werden von measurements.csv eingelesen");
var measurements = await ImportController.ReadFromCsvAsync();
if (measurements.Length == 0)
{
    Console.WriteLine("!!! Es wurden keine Measurements eingelesen");
    return;
}
Console.WriteLine($"  Es wurden {measurements.Length} Measurements eingelesen!");
var sensors = measurements.Select(m => m.Sensor).Distinct().ToList();
Console.WriteLine($"  Es wurden {sensors.Count()} Sensoren eingelesen!");
Console.WriteLine("Daten werden in Datenbank gespeichert (in Context übertragen)");
await unitOfWork.Measurements.AddRangeAsync(measurements);
int changes = await unitOfWork.SaveChangesAsync();
Console.WriteLine($"{changes} Datensätze wurden in die Datenbank gespeichert");
Console.Write("Beenden mit Eingabetaste ...");
Console.ReadLine();
