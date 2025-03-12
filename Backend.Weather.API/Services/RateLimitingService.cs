using Backend.Weather.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

public class RateLimitingService : IRateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly int _limit = 5;
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(60);
    private readonly string[] _validApiKeys = ["API_KEY_1", "API_KEY_2", "API_KEY_3", "API_KEY_4", "API_KEY_5"];
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService(IMemoryCache cache, ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public bool IsRequestAllowed(string apiKey)
    {
        // Check if the API key is valid
        if (!_validApiKeys.Contains(apiKey))
        {
            _logger.LogWarning("Invalid API key: {ApiKey}", apiKey);
            throw new BadHttpRequestException("Invalid API key.");
        }

        string cacheKey = $"rate_limit_{apiKey}";

        if (_cache.TryGetValue(cacheKey, out int requestCount))
        {
            if (requestCount >= _limit)
            {
                _logger.LogWarning("Rate limit exceeded for API key: {ApiKey}", apiKey);
                return false; // Rate limit exceeded
            }

            _cache.Set(cacheKey, requestCount + 1, DateTimeOffset.UtcNow.Add(_timeWindow));
            _logger.LogInformation("Request allowed for API key: {ApiKey}. Current count: {RequestCount}", apiKey, requestCount + 1);
        }
        else
        {
            _cache.Set(cacheKey, 1, DateTimeOffset.UtcNow.Add(_timeWindow));
            _logger.LogInformation("Request allowed for API key: {ApiKey}. Current count: 1", apiKey);
        }

        return true;
    }
}
