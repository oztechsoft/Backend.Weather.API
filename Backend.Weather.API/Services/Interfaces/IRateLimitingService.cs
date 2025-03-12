namespace Backend.Weather.API.Services.Interfaces
{
    public interface IRateLimitingService
    {
        bool IsRequestAllowed(string apiKey);
    }
}
