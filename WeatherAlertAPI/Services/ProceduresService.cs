using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace WeatherAlertAPI.Services
{    public class ProceduresService : IProceduresService    {
        private readonly IDatabaseConnection _db;
        private readonly ILogger<ProceduresService> _logger;

        public ProceduresService(IDatabaseConnection db, ILogger<ProceduresService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        public async Task<IEnumerable<T>> ExecuteStoredProcWithResultAsync<T>(string procedureName, object? parameters = null)
        {
            _logger.LogInformation("Executando stored procedure: {ProcedureName}", procedureName);

            using var conn = _db.CreateConnection();
            var result = await conn.QueryAsync<T>(
                procedureName,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
            
            _logger.LogInformation("Stored procedure {ProcedureName} executada com sucesso", procedureName);
            return result;
        }

        public async Task ExecuteStoredProcAsync(string procedureName, object? parameters = null)
        {
            _logger.LogInformation("Executando stored procedure: {ProcedureName}", procedureName);

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(
                procedureName,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
            
            _logger.LogInformation("Stored procedure {ProcedureName} executada com sucesso", procedureName);
        }
    }
}
