using Backend.Weather.API.Models;
using System;
using System.Text.Json;

namespace Backend.Weather.API.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient httpClient, IConfiguration config, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }


        public async Task<WeatherResponse> GetWeatherAsync(string city, string country)
        {
            var apiKeys = _config.GetSection("OpenWeatherMapApiKeys").Get<string[]>() ?? [];
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city},{country}&appid={apiKeys[0]}";

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
                if(weatherData?.Weather == null)
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
