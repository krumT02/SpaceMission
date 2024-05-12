using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMission.WeatherClasses
{
    internal class WeatherCriteria
    {
        public int MinTemperature { get; set; }
        public int MaxTemperature { get; set; }
        public int MaxWindSpeed { get; set; }
        public int MaxHumidity { get; set; }
        public bool AllowPrecipitation { get; set; }
        public bool AllowLightning { get; set; }
        public List<string> AllowedCloudTypes { get; set; }

        public WeatherCriteria()
        {
            // Default criteria values
            MinTemperature = 0;
            MaxTemperature = 32;
            MaxWindSpeed = 11;
            MaxHumidity = 55;
            AllowPrecipitation = false;
            AllowLightning = false;
            AllowedCloudTypes = new List<string> { "Cirrus", "Stratus" };
        }
    }
}
