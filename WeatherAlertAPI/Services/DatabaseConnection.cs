using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using WeatherAlertAPI.Configuration;

namespace WeatherAlertAPI.Services
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseConnection> _logger;

        public DatabaseConnection(IOptions<DatabaseSettings> settings, ILogger<DatabaseConnection> logger)
        {
            _connectionString = settings.Value.ConnectionString;
            _logger = logger;
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                _logger.LogInformation("Criando conexão com o banco de dados");
                var connection = new OracleConnection(_connectionString);
                connection.Open();
                _logger.LogInformation("Conexão estabelecida com sucesso");
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conexão com o banco de dados");
                throw;
            }
        }
    }
}
