using Newtonsoft.Json;
using static WebApplication1.Models.WeatherResponse.WeatherData;

namespace WebApplication1.Models
{
    public class WeatherInfo
    {

        [JsonProperty("temp_c")]
        public double temp_c { get; set; }

        [JsonProperty("is_day")]
        public bool is_day { get; set; }

        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("icon")]
        public string icon { get; set; }

        [JsonProperty("localtime")]
        public string localtime { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

    }

    public class WeatherResponse
    {
        public WeatherData Current { get; set; }
        public Location location { get; set; }

        public class WeatherData
        {
            [JsonProperty("temp_c")]
            public double TemperatureCelsius { get; set; }

            [JsonProperty("is_day")]
            public int IsDay { get; set; }

            public WeatherCondition Condition { get; set; }

            public class WeatherCondition
            {
                [JsonProperty("text")]
                public string Text { get; set; }

                [JsonProperty("icon")]
                public string IconUrl { get; set; }
            }
        }
        public class Location
        {
            [JsonProperty("localtime")]
            public string Localtime { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }
        }
    }

}
