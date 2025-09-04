using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using WeatherAlertAPI.Models;

namespace WeatherAlertAPI.Services
{
    public class PreferenciasService : IPreferenciasService
    {
        private readonly IDatabaseConnection _db;
        private readonly ILogger<PreferenciasService> _logger;

        public PreferenciasService(IDatabaseConnection db, ILogger<PreferenciasService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<PreferenciasNotificacao> CreatePreferenciaAsync(PreferenciasNotificacao preferencia)
        {
            try
            {
                if (preferencia == null)
                    throw new ArgumentNullException(nameof(preferencia));

                if (string.IsNullOrWhiteSpace(preferencia.Cidade))
                    throw new ArgumentException("Cidade é obrigatória", nameof(preferencia));

                if (string.IsNullOrWhiteSpace(preferencia.Estado))
                    throw new ArgumentException("Estado é obrigatório", nameof(preferencia));

                if (!preferencia.TemperaturaMin.HasValue && !preferencia.TemperaturaMax.HasValue)
                    throw new ArgumentException("Pelo menos um limite de temperatura deve ser definido", nameof(preferencia));

                var parameters = new DynamicParameters();
                parameters.Add("p_cidade", preferencia.Cidade);
                parameters.Add("p_estado", preferencia.Estado);
                parameters.Add("p_temperatura_min", preferencia.TemperaturaMin);
                parameters.Add("p_temperatura_max", preferencia.TemperaturaMax);
                parameters.Add("p_ativo", preferencia.Ativo ? 1 : 0);
                parameters.Add("p_id_preferencia", dbType: DbType.Int32, direction: ParameterDirection.Output);

                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync(
                    "SP_INSERIR_PREFERENCIA",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                preferencia.IdPreferencia = parameters.Get<int>("p_id_preferencia");
                preferencia.DataCriacao = DateTime.Now;
                preferencia.DataAtualizacao = DateTime.Now;

                _logger.LogInformation("Preferência criada com sucesso. ID: {IdPreferencia}", preferencia.IdPreferencia);
                return preferencia;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<PreferenciasNotificacao>> GetPreferenciasAsync(string? cidade = null, string? estado = null)
        {
            try
            {
                _logger.LogInformation("Buscando preferências. Filtros: Cidade={Cidade}, Estado={Estado}", cidade, estado);

                var parameters = new DynamicParameters();
                parameters.Add("p_cidade", cidade);
                parameters.Add("p_estado", estado);
                parameters.Add("p_preferencias", dbType: DbType.Object, direction: ParameterDirection.Output);

                using var conn = _db.CreateConnection();
                return await conn.QueryAsync<PreferenciasNotificacao>(
                    "SP_CONSULTAR_PREFERENCIAS",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PreferenciasNotificacao?> GetPreferenciaByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("ID deve ser maior que zero", nameof(id));

                var parameters = new DynamicParameters();
                parameters.Add("p_id_preferencia", id);
                parameters.Add("p_preferencia", dbType: DbType.Object, direction: ParameterDirection.Output);

                using var conn = _db.CreateConnection();
                var result = await conn.QueryFirstOrDefaultAsync<PreferenciasNotificacao>(
                    "SP_BUSCAR_PREFERENCIA_POR_ID",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    _logger.LogWarning("Preferência não encontrada. ID: {Id}", id);
                else
                    _logger.LogInformation("Preferência encontrada. ID: {Id}", id);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdatePreferenciaAsync(PreferenciasNotificacao preferencia)
        {
            try
            {
                if (preferencia == null)
                    throw new ArgumentNullException(nameof(preferencia));

                if (preferencia.IdPreferencia <= 0)
                    throw new ArgumentException("ID deve ser maior que zero", nameof(preferencia));

                if (string.IsNullOrWhiteSpace(preferencia.Cidade))
                    throw new ArgumentException("Cidade é obrigatória", nameof(preferencia));

                if (string.IsNullOrWhiteSpace(preferencia.Estado))
                    throw new ArgumentException("Estado é obrigatório", nameof(preferencia));

                var parameters = new DynamicParameters();
                parameters.Add("p_id_preferencia", preferencia.IdPreferencia);
                parameters.Add("p_cidade", preferencia.Cidade);
                parameters.Add("p_estado", preferencia.Estado);
                parameters.Add("p_temperatura_min", preferencia.TemperaturaMin);
                parameters.Add("p_temperatura_max", preferencia.TemperaturaMax);
                parameters.Add("p_ativo", preferencia.Ativo ? 1 : 0);

                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync(
                    "SP_ATUALIZAR_PREFERENCIA",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                preferencia.DataAtualizacao = DateTime.Now;
                _logger.LogInformation("Preferência atualizada com sucesso. ID: {IdPreferencia}", preferencia.IdPreferencia);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeletePreferenciaAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("ID deve ser maior que zero", nameof(id));

                var parameters = new DynamicParameters();
                parameters.Add("p_id_preferencia", id);

                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync(
                    "SP_EXCLUIR_PREFERENCIA",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("Preferência excluída com sucesso. ID: {Id}", id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IList<PreferenciasNotificacao>> ObterPreferenciasAsync(PreferenciaFiltro filtro)
        {
            // Exemplo básico: retornar lista vazia ou buscar do banco
            return new List<PreferenciasNotificacao>();
        }
    }
}
