using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace WeatherAlertAPI.Services
{    public class ProceduresService : IProceduresService
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<ProceduresService> _logger;

        public ProceduresService(DatabaseConnection db, ILogger<ProceduresService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        public async Task<IEnumerable<T>> ExecuteStoredProcWithResultAsync<T>(string procedureName, object? parameters = null)
        {
            try
            {
                _logger.LogInformation($"Executando stored procedure: {procedureName}");

                using var conn = _db.CreateConnection();
                var result = await conn.QueryAsync<T>(
                    procedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                
                _logger.LogInformation($"Stored procedure {procedureName} executada com sucesso");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao executar stored procedure {procedureName}");
                throw;
            }
        }

        public async Task ExecuteStoredProcAsync(string procedureName, object? parameters = null)
        {
            try
            {
                _logger.LogInformation($"Executando stored procedure: {procedureName}");

                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync(
                    procedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                
                _logger.LogInformation($"Stored procedure {procedureName} executada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao executar stored procedure {procedureName}");
                throw;
            }
        }
    }
}
