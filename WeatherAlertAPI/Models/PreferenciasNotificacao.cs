using System;

namespace WeatherAlertAPI.Models
{
    public class PreferenciasNotificacao
    {
        public int IdPreferencia { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public decimal? TemperaturaMin { get; set; }
        public decimal? TemperaturaMax { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
