using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceMission.WeatherClasses;

namespace SpaceMission
{
    internal class WeatherAnalyzer
    {
        public List<WeatherForecast> ReadAndTransformCsv(string filePath, string location)
        {
            List<WeatherForecast> records = new List<WeatherForecast>();
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var headers = reader.ReadLine().Split(';').Skip(1).ToArray(); // Skip the 'Day/Parameter' header

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Split(';');
                        var parameter = line[0];
                        var values = line.Skip(1).ToArray(); // Skip the parameter name

                        for (int i = 0; i < values.Length; i++)
                        {
                            if (records.Count <= i)
                            {
                                records.Add(new WeatherForecast { Day = i + 1, Location = location });
                            }
                            try
                            {
                                switch (parameter)
                                {
                                    case "Temperature":
                                        records[i].Temperature = int.Parse(values[i]);
                                        break;
                                    case "Wind (m/s)":
                                        records[i].WindSpeed = int.Parse(values[i]);
                                        break;
                                    case "Humidity(%)":
                                        records[i].Humidity = int.Parse(values[i]);
                                        break;
                                    case "Precipitation(%)":
                                        records[i].Precipitation = int.Parse(values[i]);
                                        break;
                                    case "Lighting":
                                        records[i].Lightning = values[i];
                                        break;
                                    case "Clouds":
                                        records[i].Clouds = values[i];
                                        break;
                                }
                            }
                            catch (FormatException ex)
                            {
                                Console.WriteLine($"Data format error in column '{parameter}' on day {i + 1}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or processing the file '{filePath}': {ex.Message}");
            }
            return records;
        }


        public WeatherForecast FindBestLaunchDay(List<WeatherForecast> forecasts, WeatherCriteria criteria)
        {
            // Assuming closer to equator is better, let's prioritize by hardcoded latitudes (not real, just for demo)
            var locationPriority = new Dictionary<string, int>
    {
        { "Kourou, French Guyana", 1 },
        { "Cape Canaveral, USA", 2 },
        { "Mahia, New Zealand", 3 },
        { "Kodiak, USA", 4 },
        { "Tanegashima, Japan", 5 }
    };

            var suitableDay = forecasts
                .Where(f => f.Temperature >= criteria.MinTemperature && f.Temperature <= criteria.MaxTemperature
                            && f.WindSpeed <= criteria.MaxWindSpeed
                            && f.Humidity <= criteria.MaxHumidity
                            && (criteria.AllowPrecipitation || f.Precipitation == 0)
                            && (criteria.AllowLightning || f.Lightning == "No")
                            && (criteria.AllowedCloudTypes.Count == 0 || criteria.AllowedCloudTypes.Contains(f.Clouds)))
                .OrderBy(f => locationPriority[f.Location]) // Prioritize by location
                .ThenBy(f => f.WindSpeed) // Then prioritize by the lowest windSpeed
                .ThenBy(f => f.Humidity) // Then prioritize by the lowest humidity
                .ThenBy(f => f.Day) // Next by the earliest suitable day
                .FirstOrDefault();

            return suitableDay;
        }



    }
}
