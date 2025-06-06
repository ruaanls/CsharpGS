using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using WeatherAlertAPI.Models;

namespace WeatherAlertAPI.Services
{
    public class PreferenciasService : IPreferenciasService
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<PreferenciasService> _logger;

        public PreferenciasService(DatabaseConnection db, ILogger<PreferenciasService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<PreferenciasNotificacao> CreatePreferenciaAsync(PreferenciasNotificacao preferencia)
        {
            if (preferencia == null)
                throw new ArgumentNullException(nameof(preferencia));

            if (string.IsNullOrWhiteSpace(preferencia.Cidade))
                throw new ArgumentException("Cidade é obrigatória", nameof(preferencia));

            if (string.IsNullOrWhiteSpace(preferencia.Estado))
                throw new ArgumentException("Estado é obrigatório", nameof(preferencia));

            if (!preferencia.TemperaturaMin.HasValue && !preferencia.TemperaturaMax.HasValue)
                throw new ArgumentException("Pelo menos um limite de temperatura deve ser definido", nameof(preferencia));

            const string sql = @"
                INSERT INTO PREFERENCIAS_NOTIFICACAO 
                (CIDADE, ESTADO, TEMPERATURA_MIN, TEMPERATURA_MAX, ATIVO, DATA_CRIACAO, DATA_ATUALIZACAO) 
                VALUES 
                (:Cidade, :Estado, :TemperaturaMin, :TemperaturaMax, :Ativo, :DataCriacao, :DataAtualizacao)
                RETURNING ID_PREFERENCIA INTO :IdPreferencia";

            preferencia.DataCriacao = DateTime.Now;
            preferencia.DataAtualizacao = DateTime.Now;

            using var conn = _db.CreateConnection();
            var parameters = new
            {
                preferencia.Cidade,
                preferencia.Estado,
                preferencia.TemperaturaMin,
                preferencia.TemperaturaMax,
                preferencia.Ativo,
                preferencia.DataCriacao,
                preferencia.DataAtualizacao,
                IdPreferencia = 0
            };

            await conn.ExecuteAsync(sql, parameters);
            return preferencia;
        }

        public async Task<IEnumerable<PreferenciasNotificacao>> GetPreferenciasAsync(string? cidade = null, string? estado = null)
        {
            try
            {
                _logger.LogInformation("Buscando preferências. Filtros: Cidade={Cidade}, Estado={Estado}", cidade, estado);

                var sql = @"SELECT 
                    ID_PREFERENCIA as IdPreferencia,
                    CIDADE,
                    ESTADO,
                    TEMPERATURA_MIN as TemperaturaMin,
                    TEMPERATURA_MAX as TemperaturaMax,
                    ATIVO,
                    DATA_CRIACAO as DataCriacao,
                    DATA_ATUALIZACAO as DataAtualizacao
                    FROM PREFERENCIAS_NOTIFICACAO
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(cidade))
                    sql += " AND CIDADE = :cidade";
                if (!string.IsNullOrWhiteSpace(estado))
                    sql += " AND ESTADO = :estado";

                using var conn = _db.CreateConnection();
                _logger.LogInformation("Executando query: {Sql}", sql);
                
                var result = await conn.QueryAsync<PreferenciasNotificacao>(sql, new { cidade, estado });
                _logger.LogInformation("Preferências encontradas: {Count}", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar preferências");
                throw;
            }
        }

        public async Task<PreferenciasNotificacao?> GetPreferenciaByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            const string sql = @"SELECT 
                ID_PREFERENCIA as IdPreferencia,
                CIDADE,
                ESTADO,
                TEMPERATURA_MIN as TemperaturaMin,
                TEMPERATURA_MAX as TemperaturaMax,
                ATIVO,
                DATA_CRIACAO as DataCriacao,
                DATA_ATUALIZACAO as DataAtualizacao
                FROM PREFERENCIAS_NOTIFICACAO 
                WHERE ID_PREFERENCIA = :id";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PreferenciasNotificacao>(sql, new { id });
        }

        public async Task UpdatePreferenciaAsync(PreferenciasNotificacao preferencia)
        {
            if (preferencia == null)
                throw new ArgumentNullException(nameof(preferencia));

            if (preferencia.IdPreferencia <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(preferencia));

            if (string.IsNullOrWhiteSpace(preferencia.Cidade))
                throw new ArgumentException("Cidade é obrigatória", nameof(preferencia));

            if (string.IsNullOrWhiteSpace(preferencia.Estado))
                throw new ArgumentException("Estado é obrigatório", nameof(preferencia));

            const string sql = @"
                UPDATE PREFERENCIAS_NOTIFICACAO 
                SET CIDADE = :Cidade,
                    ESTADO = :Estado,
                    TEMPERATURA_MIN = :TemperaturaMin,
                    TEMPERATURA_MAX = :TemperaturaMax,
                    ATIVO = :Ativo,
                    DATA_ATUALIZACAO = :DataAtualizacao
                WHERE ID_PREFERENCIA = :IdPreferencia";

            preferencia.DataAtualizacao = DateTime.Now;

            using var conn = _db.CreateConnection();
            var result = await conn.ExecuteAsync(sql, preferencia);
            if (result == 0)
                throw new KeyNotFoundException($"Preferência com ID {preferencia.IdPreferencia} não encontrada");
        }

        public async Task DeletePreferenciaAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero", nameof(id));

            const string sql = "DELETE FROM PREFERENCIAS_NOTIFICACAO WHERE ID_PREFERENCIA = :id";
            
            using var conn = _db.CreateConnection();
            var result = await conn.ExecuteAsync(sql, new { id });
            if (result == 0)
                throw new KeyNotFoundException($"Preferência com ID {id} não encontrada");
        }
    }
}
