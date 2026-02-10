using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;


namespace CityBikeApp
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Definicja opcji --file / -f
            var fileOption = new Option<FileInfo>(
                name: "--file",
                description: "Ścieżka do pliku CSV z danymi CitiBike")
            {
                IsRequired = true 
            };
            fileOption.AddAlias("-f");

            var rootCommand = new RootCommand("Aplikacja do analizy danych CityBike");
            rootCommand.AddOption(fileOption);
            
            rootCommand.SetHandler((fileInfo) =>
            {
                RunAnalysis(fileInfo);
            }, fileOption);

            return await rootCommand.InvokeAsync(args);
        }

        static void RunAnalysis(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd: Nie znaleziono pliku: {fileInfo.FullName}");
                Console.ResetColor();
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            List<TripData> trips;

            Console.WriteLine($"Wczytywanie danych z: {fileInfo.Name}...");

            try
            {
                using (var reader = new StreamReader(fileInfo.FullName))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<TripDataMap>();
                    trips = csv.GetRecords<TripData>().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas odczytu pliku: {ex.Message}");
                return;
            }

            Console.WriteLine($"\nZaładowano {trips.Count} przejazdów.");
            Console.WriteLine("--------------------------------------------------\n");

            // 1. Typy rowerów 
            Console.WriteLine("1. Popularność typów rowerów:");
            var bikeTypes = trips
                .GroupBy(t => t.RideableType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count);

            foreach (var bike in bikeTypes)
            {
                string name = bike.Type.Replace("_", " ");
                Console.WriteLine($"   - {name}: {bike.Count} wypożyczeń");
            }
            Console.WriteLine();

            // 2. Ruch w poszczególne dni tygodnia
            Console.WriteLine("2. Ruch w poszczególne dni tygodnia:");
            var dayStats = trips
                .GroupBy(t => t.StartedAt.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            foreach (var d in dayStats)
            {
                Console.WriteLine($"   - {d.Day}: {d.Count}");
            }
            Console.WriteLine();

            // 3. 5 najpopularniejszych stacj startowych
            Console.WriteLine("3. Top 5 stacji startowych:");
            var stations = trips
                .Where(t => !string.IsNullOrEmpty(t.StartStation))
                .GroupBy(t => t.StartStation)
                .Select(g => new { Station = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5);

            int rank = 1;
            foreach (var s in stations)
            {
                Console.WriteLine($"   {rank}. {s.Station} ({s.Count} startów)");
                rank++;
            }
            Console.WriteLine();

            // 4. member vs casual
            Console.WriteLine("4. Średni czas jazdy: Członkowie vs Reszta:");
            var memberStats = trips
                .GroupBy(t => t.MemberCasual)
                .Select(g => new
                {
                    Type = g.Key,
                    AvgTime = g.Average(x => x.DurationInMinutes)
                });

            foreach (var item in memberStats)
            {
                Console.WriteLine($"   - {item.Type}: średnio {item.AvgTime:F2} min");
            }
            Console.WriteLine();

            // 5. Liczba przejazdów dłuższych niż 45 minut
            var longRides = trips.Count(t => t.DurationInMinutes > 45);
            Console.WriteLine($"5. Liczba przejazdów dłuższych niż 45 minut: {longRides}");

            double percent = trips.Count > 0 ? (double)longRides / trips.Count * 100 : 0;
            Console.WriteLine($"   (Stanowi to {percent:F2}% wszystkich tras)");

            // 6. Top 5 najdłuższych przejazdów
            Console.WriteLine("\n6. Top 5 najdłuższych przejazdów (Rekordziści):");

            var longestTrips = trips
                .OrderByDescending(t => t.DurationInMinutes)
                .Take(5);

            int no = 1;
            foreach (var trip in longestTrips)
            {
                string endName = string.IsNullOrEmpty(trip.EndStation) ? "(Nieznana / Nie zwrócono)" : trip.EndStation;
                string startName = string.IsNullOrEmpty(trip.StartStation) ? "(Nieznana)" : trip.StartStation;

                Console.WriteLine($"   {no}. {trip.DurationInMinutes:F2} min | Trasa: {startName} -> {endName}");
                no++;
            }

            Console.WriteLine("\nKoniec analizy.");
        }
    }
}