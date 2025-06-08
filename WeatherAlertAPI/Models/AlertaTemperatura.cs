using System;

namespace WeatherAlertAPI.Models
{
    /// <summary>
    /// Represents a temperature alert for a specific city
    /// </summary>
    /// <example>
    /// {
    ///   "idAlerta": 1,
    ///   "cidade": "São Paulo",
    ///   "estado": "SP",
    ///   "temperatura": 32.5,
    ///   "tipoAlerta": "TEMPERATURA_ALTA",
    ///   "mensagem": "Temperatura acima do limite máximo configurado",
    ///   "dataHora": "2024-01-21T14:30:00",
    ///   "status": "ATIVO"
    /// }
    /// </example>
    public class AlertaTemperatura
    {
        /// <summary>
        /// Creates a new temperature alert instance
        /// </summary>
        /// <param name="cidade">The city name</param>
        /// <param name="estado">The state (UF) code</param>
        /// <param name="tipoAlerta">The type of alert (e.g., TEMPERATURA_ALTA, TEMPERATURA_BAIXA)</param>
        /// <param name="mensagem">The alert message</param>
        public AlertaTemperatura(string cidade, string estado, string tipoAlerta, string mensagem)
        {
            Cidade = cidade;
            Estado = estado;
            TipoAlerta = tipoAlerta;
            Mensagem = mensagem;
        }

        /// <summary>
        /// Default constructor for deserialization
        /// </summary>
        public AlertaTemperatura() { }

        /// <summary>
        /// The unique identifier for the alert
        /// </summary>
        public int IdAlerta { get; set; }

        /// <summary>
        /// The city where the temperature was measured
        /// </summary>
        /// <example>São Paulo</example>
        public string Cidade { get; set; } = "";

        /// <summary>
        /// The state (UF) where the city is located
        /// </summary>
        /// <example>SP</example>
        public string Estado { get; set; } = "";

        /// <summary>
        /// The measured temperature in Celsius
        /// </summary>
        /// <example>32.5</example>
        public decimal Temperatura { get; set; }

        /// <summary>
        /// The type of temperature alert (TEMPERATURA_ALTA or TEMPERATURA_BAIXA)
        /// </summary>
        /// <example>TEMPERATURA_ALTA</example>
        public string TipoAlerta { get; set; } = "";

        /// <summary>
        /// Detailed message describing the alert
        /// </summary>
        /// <example>Temperatura acima do limite máximo configurado</example>
        public string Mensagem { get; set; } = "";

        /// <summary>
        /// The date and time when the alert was generated
        /// </summary>
        public DateTime DataHora { get; set; }

        /// <summary>
        /// The current status of the alert (ATIVO or INATIVO)
        /// </summary>
        /// <example>ATIVO</example>
        public string Status { get; set; } = "ATIVO";
    }
}
