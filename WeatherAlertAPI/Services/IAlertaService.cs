using WeatherAlertAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherAlertAPI.Services
{
    public interface IAlertaService
    {
        Task<AlertaTemperatura> CreateAlertaAsync(AlertaTemperatura alerta);
        Task<IEnumerable<AlertaTemperatura>> GetAlertasAsync(string? cidade = null, string? estado = null);
        Task<AlertaTemperatura> GetAlertaByIdAsync(int id);
        Task UpdateAlertaStatusAsync(int id, string status);
    }
}
