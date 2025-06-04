using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WeatherAlertAPI.Models;

namespace WeatherAlertAPI.Services
{
    public class PreferenciasService : IPreferenciasService
    {
        private readonly DatabaseConnection _db;

        public PreferenciasService(DatabaseConnection db)
        {
            _db = db;
        }

        public async Task<PreferenciasNotificacao> CreatePreferenciaAsync(PreferenciasNotificacao preferencia)
        {
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

            if (!string.IsNullOrEmpty(cidade))
                sql += " AND CIDADE = :cidade";
            if (!string.IsNullOrEmpty(estado))
                sql += " AND ESTADO = :estado";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<PreferenciasNotificacao>(sql, new { cidade, estado });
        }

        public async Task<PreferenciasNotificacao> GetPreferenciaByIdAsync(int id)
        {
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
            await conn.ExecuteAsync(sql, preferencia);
        }

        public async Task DeletePreferenciaAsync(int id)
        {
            const string sql = "DELETE FROM PREFERENCIAS_NOTIFICACAO WHERE ID_PREFERENCIA = :id";
            
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, new { id });
        }
    }
}
