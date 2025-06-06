using System;

namespace WeatherAlertAPI.Models
{
    public class AlertaTemperatura
    {
        public AlertaTemperatura(string cidade, string estado, string tipoAlerta, string mensagem)
        {
            Cidade = cidade;
            Estado = estado;
            TipoAlerta = tipoAlerta;
            Mensagem = mensagem;
        }        // Constructor for deserialization
        public AlertaTemperatura() { }

        public int IdAlerta { get; set; }
        public string Cidade { get; set; } = "";
        public string Estado { get; set; } = "";
        public decimal Temperatura { get; set; }        public string TipoAlerta { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public DateTime DataHora { get; set; }
        public string Status { get; set; } = "ATIVO";
    }
}
