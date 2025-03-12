using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Backend.Weather.API.Tests.ServiceTests
{
    public class RateLimitingServiceTests
    {
        private readonly RateLimitingService _rateLimitingService;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<RateLimitingService>> _mockLogger;

        public RateLimitingServiceTests()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<RateLimitingService>>();
            _rateLimitingService = new RateLimitingService(_mockCache.Object, _mockLogger.Object);
        }

        [Fact]
        public void IsRequestAllowed_InvalidApiKey_ThrowsException()
        {
            // Arrange
            var apiKey = "invalid_api_key";

            // Act & Assert
            Assert.Throws<BadHttpRequestException>(() => _rateLimitingService.IsRequestAllowed(apiKey));
        }

        [Fact]
        public void IsRequestAllowed_ValidApiKey_ReturnsTrue()
        {
            // Arrange
            var apiKey = "API_KEY_1";
            var cacheEntry = Mock.Of<ICacheEntry>();
            _mockCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Returns(false);
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            // Act
            var result = _rateLimitingService.IsRequestAllowed(apiKey);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsRequestAllowed_ExceedsRateLimit_ThrowsException()
        {
            // Arrange
            var apiKey = "API_KEY_1";
            var cacheKey = $"rate_limit_{apiKey}";
            var cacheEntry = Mock.Of<ICacheEntry>();
            object cacheValue = 5;
            _mockCache.Setup(m => m.TryGetValue(cacheKey, out cacheValue)).Returns(true);
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            // Act
            var result = _rateLimitingService.IsRequestAllowed(apiKey);

            // Assert
            Assert.False(result);
        }

    }
}
