using Backend.Weather.API.Models;
using Backend.Weather.API.Services.Interfaces;
using System;
using System.Text.Json;

namespace Backend.Weather.API.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private readonly string[] _openWeatherMapApiKeys = ["8b7535b42fe1c551f18028f64e8688f7", "9f933451cebf1fa39de168a29a4d9a79"];
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<WeatherResponse> GetWeatherAsync(string city, string country)
        {
            var url = $"{_baseUrl}?q={city},{country}&appid={_openWeatherMapApiKeys[0]}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get weather data for {City}, {Country}. Status Code: {StatusCode}", city, country, response.StatusCode);
                    throw new HttpRequestException($"Failed to get weather data. Status Code: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(content);
                if(weatherData?.Weather == null || weatherData?.Weather.Count == 0)
                {
                    _logger.LogWarning("No weather details found for {City}, {Country}.", city, country);
                    return null;
                }

                var description = string.Join(", ", weatherData.Weather.Select(w => w.Description));

                return new WeatherResponse
                {
                    Description = description,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting weather data for {City}, {Country}.", city, country);
                throw;
            }
        }
    }
}
