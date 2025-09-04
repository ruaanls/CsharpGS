using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using WeatherAlertAPI.Models;
using Oracle.ManagedDataAccess.Client;
using Dapper.Oracle;

namespace WeatherAlertAPI.Services
{
    public class AlertaService : IAlertaService
    {
        private readonly IDatabaseConnection _db;
        private readonly ILogger<AlertaService> _logger;

        public AlertaService(IDatabaseConnection db, ILogger<AlertaService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AlertaTemperatura> CreateAlertaAsync(AlertaTemperatura alerta)
        {
            _logger.LogInformation("Criando alerta de temperatura para {Cidade}/{Estado}", alerta.Cidade, alerta.Estado);

            var parameters = new OracleDynamicParameters();
            parameters.Add("p_cidade", alerta.Cidade);
            parameters.Add("p_estado", alerta.Estado);
            parameters.Add("p_temperatura", alerta.Temperatura);
            parameters.Add("p_tipo_alerta", alerta.TipoAlerta);
            parameters.Add("p_mensagem", alerta.Mensagem);
            parameters.Add("p_id_alerta", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(
                "SP_INSERIR_ALERTA",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            alerta.IdAlerta = parameters.Get<int>("p_id_alerta");
            alerta.DataHora = DateTime.Now;
            alerta.Status = "ATIVO";

            _logger.LogInformation("Alerta criado com sucesso. ID: {IdAlerta}", alerta.IdAlerta);
            return alerta;
        }

        public async Task<IEnumerable<AlertaTemperatura>> GetAlertasAsync(string? cidade = null, string? estado = null)
        {
            _logger.LogInformation("Buscando alertas. Filtros: Cidade={Cidade}, Estado={Estado}", cidade, estado);

            var parameters = new OracleDynamicParameters();
            parameters.Add("p_cidade", cidade);
            parameters.Add("p_estado", estado);
            parameters.Add("p_alertas", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<AlertaTemperatura>(
                "SP_CONSULTAR_ALERTAS",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<AlertaTemperatura?> GetAlertaByIdAsync(int id)
        {
            _logger.LogInformation("Buscando alerta por ID: {Id}", id);

            var parameters = new OracleDynamicParameters();
            parameters.Add("p_id_alerta", id);
            parameters.Add("p_alerta", null, dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

            using var conn = _db.CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<AlertaTemperatura>(
                "SP_BUSCAR_ALERTA_POR_ID",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
                _logger.LogWarning("Alerta não encontrado. ID: {Id}", id);
            else
                _logger.LogInformation("Alerta encontrado. ID: {Id}", id);

            return result;
        }

        public async Task UpdateAlertaStatusAsync(int id, string status)
        {
            _logger.LogInformation("Atualizando status do alerta {Id} para {Status}", id, status);

            var parameters = new DynamicParameters();
            parameters.Add("p_id_alerta", id);
            parameters.Add("p_status", status);

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(
                "SP_ATUALIZAR_STATUS_ALERTA",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            _logger.LogInformation("Status do alerta atualizado com sucesso");
        }
    }
}
