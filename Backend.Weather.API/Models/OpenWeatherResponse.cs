namespace Backend.Weather.API.Models
{
    public class OpenWeatherResponse
    {
        public List<Weather> weather { get; set; }
    }

    public class Weather
    {
        public string description { get; set; }
    }
}
