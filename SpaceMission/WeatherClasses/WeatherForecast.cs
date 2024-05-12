using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMission.WeatherClasses
{
    internal class WeatherForecast
    {
        public string Location { get; set; }
        public int Day { get; set; }
        public int Temperature { get; set; }
        public int WindSpeed { get; set; }
        public int Humidity { get; set; }
        public int Precipitation { get; set; }
        public string Lightning { get; set; }
        public string Clouds { get; set; }
    }
}
