using Backend.Weather.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Weather.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly ILogger<WeatherController> _logger;
    private readonly IWeatherService _weatherService;
    private readonly RateLimitingService _rateLimitingService;

    public WeatherController(IWeatherService weatherService, RateLimitingService rateLimitingService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeather")]
    public async Task<IActionResult> GetWeather(string city, string country, [FromHeader] string apiKey)
    {
        try
        {
            if (!_rateLimitingService.IsRequestAllowed(apiKey))
            {
                return BadRequest("hourly limit has been exceeded");
            }

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
            {
                _logger.LogWarning("City or country parameter is missing.");
                return BadRequest("City and country parameters are required.");
            }

            var weather = await _weatherService.GetWeatherAsync(city, country);
            if (weather == null)
            {
                _logger.LogWarning("Weather data not found for {City}, {Country}.", city, country);
                return NotFound("Weather data not found.");
            }

            return Ok(weather);
        }
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(ex, "Bad Request: {Message}.", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting weather data for {City}, {Country}.", city, country);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
