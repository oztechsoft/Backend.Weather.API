﻿using Backend.Weather.API.Models;

namespace Backend.Weather.API.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponse> GetWeatherAsync(string city, string country);
    }
}
