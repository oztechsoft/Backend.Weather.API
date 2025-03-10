using System.Text.Json.Serialization;

namespace Backend.Weather.API.Models
{
    public class OpenWeatherResponse
    {
        [JsonPropertyName("weather")]
        public List<Weather> Weather { get; set; }
    }

    public class Weather
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
