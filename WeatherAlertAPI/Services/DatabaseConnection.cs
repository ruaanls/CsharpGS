using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Options;
using WeatherAlertAPI.Configuration;

namespace WeatherAlertAPI.Services
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IOptions<DatabaseSettings> settings)
        {
            _connectionString = settings.Value.ConnectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }
    }
}
