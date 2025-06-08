using System;

namespace WeatherAlertAPI.Models
{
    public class DadosChuva
    {
        public int Id { get; set; }
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public decimal TemperaturaMedia { get; set; }
        public decimal TotalPrecipitacao { get; set; }
        public decimal ProbabilidadeDeChuva { get; set; }
        public string Conclusao { get; set; } = string.Empty;
        public string IdUsuario { get; set; } = string.Empty;
    }
} 