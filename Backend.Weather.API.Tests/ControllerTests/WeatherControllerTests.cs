using Moq;
using Microsoft.Extensions.Logging;
using Backend.Weather.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Backend.Weather.API.Models;
using Microsoft.AspNetCore.Http;
using Backend.Weather.API.Services.Interfaces;

namespace Backend.Weather.API.Tests.ControllerTests
{
    public class WeatherControllerTests
    {
        private readonly WeatherController _controller;
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly Mock<IRateLimitingService> _mockRateLimitingService;
        private readonly Mock<ILogger<WeatherController>> _mockLogger;

        public WeatherControllerTests()
        {
            _mockWeatherService = new Mock<IWeatherService>();
            _mockRateLimitingService = new Mock<IRateLimitingService>();
            _mockLogger = new Mock<ILogger<WeatherController>>();
            _controller = new WeatherController(_mockWeatherService.Object, _mockRateLimitingService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetWeather_ValidRequest_ReturnsOk()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var apiKey = "valid_api_key";
            _mockRateLimitingService.Setup(x => x.IsRequestAllowed(apiKey)).Returns(true);
            _mockWeatherService.Setup(x => x.GetWeatherAsync(city, country)).ReturnsAsync(new WeatherResponse { Description = "sunny" });

            // Act
            var result = await _controller.GetWeather(city, country, apiKey);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var weatherResponse = Assert.IsType<WeatherResponse>(okResult.Value);
            Assert.Equal("sunny", weatherResponse.Description);
        }

        [Fact]
        public async Task GetWeather_InvalidApiKey_ReturnsBadRequest()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var apiKey = "invalid_api_key";
            _mockRateLimitingService.Setup(x => x.IsRequestAllowed(apiKey)).Throws(new BadHttpRequestException("Invalid API key."));

            // Act
            var result = await _controller.GetWeather(city, country, apiKey);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid API key.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetWeather_ExceedsRateLimit_ReturnsTooManyRequests()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var apiKey = "valid_api_key";
            _mockRateLimitingService.Setup(x => x.IsRequestAllowed(apiKey)).Returns(false);

            // Act
            var result = await _controller.GetWeather(city, country, apiKey);

            // Assert
            var tooManyRequestsResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(429, tooManyRequestsResult.StatusCode);
            Assert.Equal("Hourly rate limit has been exceeded.", tooManyRequestsResult.Value);
        }

        [Fact]
        public async Task GetWeather_NullCityOrCountry_ReturnsBadRequest()
        {
            // Arrange
            string city = null;
            var country = "UK";
            var apiKey = "API_KEY_1";
            _mockRateLimitingService.Setup(x => x.IsRequestAllowed(apiKey)).Returns(true);

            // Act
            var result = await _controller.GetWeather(city, country, apiKey);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("City and country parameters are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetWeather_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var apiKey = "API_KEY_1";
            _mockRateLimitingService.Setup(x => x.IsRequestAllowed(apiKey)).Returns(true);
            _mockWeatherService.Setup(x => x.GetWeatherAsync(city, country)).ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetWeather(city, country, apiKey);

            // Assert
            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
            Assert.Equal("An error occurred while processing your request.", internalServerErrorResult.Value);
        }

    }

}
