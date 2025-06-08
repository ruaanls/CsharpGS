using System;

namespace WeatherAlertAPI.Models
{
    public class Usuario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public int? IdDadosChuva { get; set; }
    }
} 