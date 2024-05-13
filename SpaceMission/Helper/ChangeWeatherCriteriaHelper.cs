using Newtonsoft.Json;
using SpaceMission.WeatherClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMission.Helper
{
    internal class ChangeWeatherCriteriaHelper
    {
        ResourceManager rm { get; set; }
        CultureInfo ci { get; set; }
        public ChangeWeatherCriteriaHelper(ResourceManager resourceManager, CultureInfo cultureinfo)
        {
            rm = resourceManager;
            ci = cultureinfo;


        }
        public void ChangeWeatherCriteria(Dictionary<string, WeatherCriteria> spaceportCriteria)
        {
            Console.WriteLine(rm.GetString("SpaceportUpdate", ci));
            var spaceports = spaceportCriteria.Keys.ToList();
            for (int i = 0; i < spaceports.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {spaceports[i]}");
            }

            var input = Console.ReadLine();
            var indices = input.Split(',').Select(s =>
            {
                bool success = int.TryParse(s.Trim(), out int index);
                return new { success, index };
            });

            foreach (var entry in indices)
            {
                if (!entry.success || entry.index < 1 || entry.index > spaceports.Count)
                {
                    Console.WriteLine(rm.GetString("IndexOutOfRange", ci) + entry.index);
                    continue;
                }
                string selectedSpaceport = spaceports[entry.index - 1];
                WeatherCriteria criteria = spaceportCriteria[selectedSpaceport];
                Console.WriteLine(rm.GetString("UpdatingCriteria", ci) + selectedSpaceport);
                UpdateCriteria(criteria);
                Console.WriteLine(rm.GetString("SaveFile",ci));
                string answear = Console.ReadLine();
                if(answear == "y") 
                SaveCriteriaToFile(spaceportCriteria);
            }
        }

        private void UpdateCriteria(WeatherCriteria criteria)
        {
            criteria.MinTemperature = ReadIntFromConsole(rm.GetString("MinTemp", ci), criteria.MinTemperature);
            criteria.MaxTemperature = ReadIntFromConsole(rm.GetString("MaxTemp", ci), criteria.MaxTemperature);
            criteria.MaxWindSpeed = ReadIntFromConsole(rm.GetString("MaxWind", ci), criteria.MaxWindSpeed);
            criteria.MaxHumidity = ReadIntFromConsole(rm.GetString("MaxHumidity", ci), criteria.MaxHumidity);
            criteria.AllowPrecipitation = ReadBooleanFromConsole(rm.GetString("Precipation", ci), criteria.AllowPrecipitation);
            criteria.AllowLightning = ReadBooleanFromConsole(rm.GetString("Lighting", ci), criteria.AllowLightning);
            criteria.AllowedCloudTypes = ReadListFromConsole(rm.GetString("CloudTypes", ci), criteria.AllowedCloudTypes);

            Console.WriteLine(rm.GetString("CriteriaUpdateW", ci));
            

        }

        private int ReadIntFromConsole(string prompt, int currentValue)
        {
            while (true)
            {
                Console.WriteLine($"{prompt} (current: {currentValue}):");
                if (int.TryParse(Console.ReadLine(), out int result))
                {
                    return result;
                }
                Console.WriteLine(rm.GetString("InvalidInput",ci));
            }
        }

        private bool ReadBooleanFromConsole(string prompt, bool currentValue)
        {
            while (true)
            {
                Console.WriteLine($"{prompt} (yes/no, current: {(currentValue ? "yes" : "no")}):");
                var input = Console.ReadLine().Trim().ToLower();
                if (input == "yes") return true;
                if (input == "no") return false;
                Console.WriteLine(rm.GetString("InvalidInput",ci));
            }
        }

        private readonly List<string> validCloudTypes = new List<string>
        {
            "Cirrus", "Cumulus", "Stratus", "Nimbus"
        };
        private List<string> ReadListFromConsole(string prompt, List<string> currentValues)
        {
            Console.WriteLine($"{prompt} (current: {string.Join(", ", currentValues)}):");
            while (true)
            {
                var input = Console.ReadLine().Trim();
                if (string.IsNullOrEmpty(input))
                {
                    return new List<string>();  // If the input is empty, assume no restriction if that's the intended behavior.
                }

                var inputs = input.Split(',').Select(s => s.Trim()).ToList();
                var invalidEntries = inputs.Except(validCloudTypes).ToList();
                if (invalidEntries.Count > 0)
                {
                    string Invalid = string.Format(ci, rm.GetString("InvalidCloudTypes", ci), string.Join(", ", invalidEntries), string.Join(", ", validCloudTypes));
                    Console.WriteLine(Invalid);
                    Console.WriteLine(rm.GetString("ValidTypes", ci));
                }
                else
                {
                    return inputs;  // Return the list of valid inputs
                }
            }
        }
        private static void SaveCriteriaToFile(Dictionary<string, WeatherCriteria> spaceportCriteria)
        {
            string filePath = "WeatherCriteria.json";
            string json = JsonConvert.SerializeObject(spaceportCriteria, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Console.WriteLine("Weather criteria saved successfully.");
        }
    }
}
