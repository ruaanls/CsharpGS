using WeatherAlertAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherAlertAPI.Services
{
    public interface IPreferenciasService
    {
        Task<PreferenciasNotificacao> CreatePreferenciaAsync(PreferenciasNotificacao preferencia);
        Task<IEnumerable<PreferenciasNotificacao>> GetPreferenciasAsync(string? cidade = null, string? estado = null);
        Task<PreferenciasNotificacao?> GetPreferenciaByIdAsync(int id);
        Task UpdatePreferenciaAsync(PreferenciasNotificacao preferencia);
        Task DeletePreferenciaAsync(int id);
    }
}
