using Microsoft.Extensions.Caching.Memory;

public class RateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly int _limit;
    private readonly TimeSpan _timeWindow;
    private readonly string[] _validApiKeys;
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService(IMemoryCache cache, IConfiguration config, ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _limit = config.GetValue<int>("ApiSettings:RateLimit");
        _timeWindow = TimeSpan.FromMinutes(config.GetValue<int>("ApiSettings:RateLimitPeriodInMins"));
        _validApiKeys = config.GetSection("ApiSettings:ApiKeys").Get<string[]>() ?? [];
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
