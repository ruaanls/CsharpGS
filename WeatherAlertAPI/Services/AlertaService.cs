using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WeatherAlertAPI.Models;

namespace WeatherAlertAPI.Services
{
    public class AlertaService : IAlertaService
    {
        private readonly DatabaseConnection _db;

        public AlertaService(DatabaseConnection db)
        {
            _db = db;
        }

        public async Task<AlertaTemperatura> CreateAlertaAsync(AlertaTemperatura alerta)
        {
            const string sql = @"
                INSERT INTO ALERTAS_TEMPERATURA 
                (CIDADE, ESTADO, TEMPERATURA, TIPO_ALERTA, MENSAGEM, DATA_HORA, STATUS) 
                VALUES 
                (:Cidade, :Estado, :Temperatura, :TipoAlerta, :Mensagem, :DataHora, :Status) 
                RETURNING ID_ALERTA INTO :IdAlerta";

            using var conn = _db.CreateConnection();
            var parameters = new
            {
                alerta.Cidade,
                alerta.Estado,
                alerta.Temperatura,
                alerta.TipoAlerta,
                alerta.Mensagem,
                alerta.DataHora,
                alerta.Status,
                IdAlerta = 0
            };

            await conn.ExecuteAsync(sql, parameters);
            return alerta;
        }

        public async Task<IEnumerable<AlertaTemperatura>> GetAlertasAsync(string? cidade = null, string? estado = null)
        {
            var sql = @"SELECT 
                ID_ALERTA as IdAlerta,
                CIDADE,
                ESTADO,
                TEMPERATURA,
                TIPO_ALERTA as TipoAlerta,
                MENSAGEM,
                DATA_HORA as DataHora,
                STATUS
                FROM ALERTAS_TEMPERATURA
                WHERE 1=1";

            if (!string.IsNullOrEmpty(cidade))
                sql += " AND CIDADE = :cidade";
            if (!string.IsNullOrEmpty(estado))
                sql += " AND ESTADO = :estado";

            sql += " ORDER BY DATA_HORA DESC";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<AlertaTemperatura>(sql, new { cidade, estado });
        }

        public async Task<AlertaTemperatura> GetAlertaByIdAsync(int id)
        {
            const string sql = @"SELECT 
                ID_ALERTA as IdAlerta,
                CIDADE,
                ESTADO,
                TEMPERATURA,
                TIPO_ALERTA as TipoAlerta,
                MENSAGEM,
                DATA_HORA as DataHora,
                STATUS
                FROM ALERTAS_TEMPERATURA 
                WHERE ID_ALERTA = :id";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AlertaTemperatura>(sql, new { id });
        }

        public async Task UpdateAlertaStatusAsync(int id, string status)
        {
            const string sql = "UPDATE ALERTAS_TEMPERATURA SET STATUS = :status WHERE ID_ALERTA = :id";
            
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, new { id, status });
        }
    }
}
