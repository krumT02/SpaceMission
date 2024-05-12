using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using CsvHelper.Configuration.Attributes;
using System.Resources;
using SpaceMission.WeatherClasses;
using SpaceMission.Helper;
using Newtonsoft.Json;


namespace SpaceMission
{
    internal class Program
    {
        private static ResourceManager rm = new ResourceManager("SpaceMission.LanguageResources.Messages", typeof(Program).Assembly);
        static void Main(string[] args)
        {

            Dictionary<string, WeatherCriteria> spaceportCriteria = LoadCriteriaFromFile();

            // Check if the loaded criteria are empty and initialize them if they are
            if (spaceportCriteria.Count == 0)
            {
                // Correctly initialize the dictionary with new entries
                spaceportCriteria = new Dictionary<string, WeatherCriteria>
    {
        { "Kourou, French Guyana", new WeatherCriteria() },
        { "Cape Canaveral, USA", new WeatherCriteria() },
        { "Mahia, New Zealand", new WeatherCriteria() },
        { "Kodiak, USA", new WeatherCriteria() },
        { "Tanegashima, Japan", new WeatherCriteria() }
    };
            }
            Console.WriteLine("Choose language: 1 for English, 2 for German");
            Console.WriteLine("The German translation was done with Google translate, so I apologize if there are any mistakes.");
            var languageChoice = Console.ReadLine();
            CultureInfo ci = languageChoice == "2" ? new CultureInfo("de") : CultureInfo.InvariantCulture;
            EmailHelper em = new EmailHelper(rm,ci);
            ChangeWeatherCriteriaHelper UpdateWeather = new ChangeWeatherCriteriaHelper(rm,ci);
            
            bool exit = false;

            do
            {
                Console.WriteLine(rm.GetString("HelloWelcome", ci));
                Console.Write(rm.GetString("ChooseOption", ci) + "\n" +
                              rm.GetString("Option1", ci) + "\n" +
                              rm.GetString("Option2", ci) + "\n" +
                              rm.GetString("Option3", ci) + "\n");
                switch (Console.ReadLine())
                {
                    case "1":
                        var result = ChooseBestDayAndPort(spaceportCriteria, ci);
                        if (result != null) { em.Email(result); }

                        break;
                    case "2":
                        UpdateWeather.ChangeWeatherCriteria(spaceportCriteria);
                        break;
                    case "3": exit = true;
                        break;
                    default:
                        Console.WriteLine(rm.GetString("InvalidChoice", ci));
                        break;
                }
                
            }while(!exit);
        }
        private static List<LaunchResult> ChooseBestDayAndPort(Dictionary<string, WeatherCriteria> spaceportCriteria, CultureInfo ci)
        {
            WeatherAnalyzer analyzer = new WeatherAnalyzer();
            string[] locations = { "Kourou, French Guyana", "Cape Canaveral, USA", "Mahia, New Zealand", "Kodiak, USA", "Tanegashima, Japan" };
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Adjusting basePath to point to the project root from the bin folder
            string adjustedBasePath = Path.GetFullPath(Path.Combine(basePath, @"..\..\.."));

            Dictionary<string, string> locationFiles = new Dictionary<string, string>
            {
                 { "Kourou, French Guyana", Path.Combine(adjustedBasePath, @"LocationsWeatherReports\Kourou, French Guyana.csv") },
                 { "Cape Canaveral, USA", Path.Combine(adjustedBasePath, @"LocationsWeatherReports\Cape Canaveral, USA.csv") },
                 { "Mahia, New Zealand", Path.Combine(adjustedBasePath, @"LocationsWeatherReports\Mahia, New Zealand.csv") },
                 { "Kodiak, USA", Path.Combine(adjustedBasePath, @"LocationsWeatherReports\Kodiak, USA.csv") },
                 { "Tanegashima, Japan", Path.Combine(adjustedBasePath, @"LocationsWeatherReports\Tanegashima, Japan.csv") }
            };

            List<WeatherForecast> allForecasts = new List<WeatherForecast>();
            Console.WriteLine(rm.GetString("LocChooice",ci));
            int index = 1;
            foreach (var loc in locations)
            {
                Console.WriteLine($"{index++}. {loc}");
            }

            var choices = Console.ReadLine().Split(',').Select(int.Parse);
            foreach (var choice in choices)
            {
                try
                {
                    string location = locations[choice - 1];
                    string filename = locationFiles[location];
                    try
                    {
                        var forecasts = analyzer.ReadAndTransformCsv(filename, location);
                        allForecasts.AddRange(forecasts);
                    }

                    catch (FileNotFoundException)
                    {
                        Console.WriteLine(rm.GetString("FileNotFound",ci));
                        return null;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine(rm.GetString("InvalidInput",ci));
                    return null;
                }
            }

            WeatherForecast bestDay = null;
            foreach (var location in choices.Select(choice => locations[choice - 1]))
            {
                List<WeatherForecast> locationForecasts = allForecasts.Where(f => f.Location == location).ToList();
                WeatherCriteria criteriaForLocation = spaceportCriteria[location]; 

                WeatherForecast bestDayForLocation = analyzer.FindBestLaunchDay(locationForecasts, criteriaForLocation);

                if (bestDayForLocation != null && (bestDay == null || bestDayForLocation.Day < bestDay.Day))
                {
                    bestDay = bestDayForLocation;
                }
            }
            if (bestDay != null)
            {
                string message = string.Format(ci, rm.GetString("BestDay", ci), bestDay.Day, bestDay.Location);
                Console.WriteLine(message);
                var result = new List<LaunchResult> { new LaunchResult { Spaceport = bestDay.Location, BestLaunchDay = "Day " + bestDay.Day } };
                WriteResultsToCsv(result,ci);
                return result;
            }
            else
            {
                Console.WriteLine(rm.GetString("NoSuitableDay", ci));
                var result = new List<LaunchResult> { new LaunchResult { Spaceport = "N/A", BestLaunchDay = rm.GetString("NoSuitableDay", ci) } };
                WriteResultsToCsv(result,ci);
                return result;
            }
        }
        private static void WriteResultsToCsv(List<LaunchResult> results,CultureInfo ci)
        {
            try
            {
                using (var writer = new StreamWriter("LaunchAnalysisReport.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(results);
                }
                Console.WriteLine(rm.GetString("CSVFileW",ci));
            }
            catch (Exception )
            {
                Console.WriteLine(rm.GetString("CSVFileL",ci));
            }
        }
      



        private static Dictionary<string, WeatherCriteria> LoadCriteriaFromFile()
        {
            string filePath = "WeatherCriteria.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, WeatherCriteria>>(json);
            }
            else
            {
                
                return new Dictionary<string, WeatherCriteria>();
            }
        }



    }
}

