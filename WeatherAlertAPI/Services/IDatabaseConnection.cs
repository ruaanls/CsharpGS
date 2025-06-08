using System.Data;

namespace WeatherAlertAPI.Services
{
    public interface IDatabaseConnection
    {
        IDbConnection CreateConnection();
    }
}
