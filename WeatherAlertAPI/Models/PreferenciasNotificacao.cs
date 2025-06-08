using System;

namespace WeatherAlertAPI.Models
{    /// <summary>
    /// Preferências do usuário para notificações de alertas meteorológicos
    /// </summary>
    /// <example>
    /// {
    ///   "idPreferencia": 1,
    ///   "cidade": "São Paulo",
    ///   "estado": "SP",
    ///   "temperaturaMin": 15.5,
    ///   "temperaturaMax": 30.0,
    ///   "ativo": true,
    ///   "dataCriacao": "2024-01-20T10:00:00",
    ///   "dataAtualizacao": "2024-01-21T15:30:00"
    /// }
    /// </example>
    public class PreferenciasNotificacao
    {
        /// <summary>
        /// Identificador único da preferência de notificação
        /// </summary>
        public int IdPreferencia { get; set; }

        /// <summary>
        /// Cidade para monitoramento de alertas
        /// </summary>
        /// <example>São Paulo</example>
        public required string Cidade { get; set; }

        /// <summary>
        /// Estado (UF) onde a cidade está localizada
        /// </summary>
        /// <example>SP</example>
        public required string Estado { get; set; }

        /// <summary>
        /// Limite mínimo de temperatura para alertas (em Celsius)
        /// </summary>
        /// <example>15.5</example>
        public decimal? TemperaturaMin { get; set; }

        /// <summary>
        /// The maximum temperature threshold for alerts (in Celsius)
        /// </summary>
        /// <example>30.0</example>
        public decimal? TemperaturaMax { get; set; }

        /// <summary>
        /// Indicates whether this notification preference is active
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// The date and time when this preference was created
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// The date and time when this preference was last updated
        /// </summary>
        public DateTime? DataAtualizacao { get; set; }
    }
}
