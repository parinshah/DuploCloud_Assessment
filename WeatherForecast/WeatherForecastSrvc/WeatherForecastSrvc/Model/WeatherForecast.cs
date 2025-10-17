using System.Text.Json.Serialization;

namespace WeatherForecastSrvc.Model
{
    public class WeatherForecast
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonPropertyName("current_weather")]
        public CurrentWeather? CurrentWeather { get; set; }
    }

    public class CurrentWeather
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("windspeed")]
        public double Windspeed { get; set; }

        [JsonPropertyName("winddirection")]
        public double Winddirection { get; set; }

        [JsonPropertyName("weathercode")]
        public int Weathercode { get; set; }

        // JSON has is_day = 0/1, so keeping it as int and not bool
        [JsonPropertyName("is_day")]
        public int IsDay { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}