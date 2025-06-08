using System.Threading.Tasks;
using System.Collections.Generic;

namespace WeatherAlertAPI.Services
{    public interface IProceduresService
    {
        Task<IEnumerable<T>> ExecuteStoredProcWithResultAsync<T>(string procedureName, object? parameters = null);
        Task ExecuteStoredProcAsync(string procedureName, object? parameters = null);
    }
}
