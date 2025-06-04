using WeatherAlertAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherAlertAPI.Services
{
    public interface IWeatherService
    {
        Task<(decimal temperatura, string cidade, string estado)> GetCurrentTemperatureAsync(string cidade, string estado);
        Task CheckAndCreateAlertsAsync();
    }
}
