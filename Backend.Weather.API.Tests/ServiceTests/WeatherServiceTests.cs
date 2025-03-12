using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Weather.API.Models;
using Backend.Weather.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Weather.API.Tests.ServiceTests
{
    public class WeatherServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<WeatherService>> _mockLogger;
        private readonly WeatherService _weatherService;

        public WeatherServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockLogger = new Mock<ILogger<WeatherService>>();

            _weatherService = new WeatherService(_httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task GetWeatherAsync_ValidRequest_ReturnsWeatherResponse()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var expectedResponse = new OpenWeatherResponse
            {
                Weather = new List<Models.Weather>
                {
                    new Models.Weather { Description = "sunny" }
                }
            };
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _weatherService.GetWeatherAsync(city, country);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("sunny", result.Description);
        }

        [Fact]
        public async Task GetWeatherAsync_HttpRequestFails_ThrowsException()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _weatherService.GetWeatherAsync(city, country));
        }

        [Fact]
        public async Task GetWeatherAsync_NoWeatherDetails_ReturnsNull()
        {
            // Arrange
            var city = "London";
            var country = "UK";
            var expectedResponse = new OpenWeatherResponse
            {
                Weather = new List<Models.Weather>()
            };
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _weatherService.GetWeatherAsync(city, country);

            // Assert
            Assert.Null(result);
        }
    }
}
