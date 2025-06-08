using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WeatherAlertAPI.Configuration;

namespace WeatherAlertAPI.Services
{
    public class WeatherService : IWeatherService
    {        private readonly HttpClient _httpClient;
        private readonly WeatherApiSettings _settings;
        private readonly IAlertaService _alertaService;
        private readonly IPreferenciasService _preferenciasService;
        private readonly ILogger<WeatherService> _logger;        public WeatherService(
            IHttpClientFactory httpClientFactory,
            IOptions<WeatherApiSettings> settings,
            IAlertaService alertaService,
            IPreferenciasService preferenciasService,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _settings = settings.Value;
            _alertaService = alertaService;
            _preferenciasService = preferenciasService;
            _logger = logger;
        }

        public async Task<(decimal temperatura, string cidade, string estado)> GetCurrentTemperatureAsync(string cidade, string estado)
        {
            var location = $"{cidade},{estado}";
            var url = $"{_settings.BaseUrl}/current.json?key={_settings.ApiKey}&q={location}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
              var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var current = json["current"];
            if (current == null)
                throw new InvalidOperationException("Dados de temperatura não disponíveis");

            var tempToken = current["temp_c"];
            if (tempToken == null)
                throw new InvalidOperationException("Temperatura não disponível");

            var temperatura = tempToken.Value<decimal>();
            
            return (temperatura, cidade, estado);
        }

        public async Task CheckAndCreateAlertsAsync()
        {
            var preferencias = await _preferenciasService.GetPreferenciasAsync();
            
            foreach (var pref in preferencias)
            {
                if (!pref.Ativo) continue;

                var (temperatura, cidade, estado) = await GetCurrentTemperatureAsync(pref.Cidade, pref.Estado);

                if (pref.TemperaturaMax.HasValue && temperatura > pref.TemperaturaMax.Value)
                {
                    await _alertaService.CreateAlertaAsync(new Models.AlertaTemperatura
                    {
                        Cidade = cidade,
                        Estado = estado,
                        Temperatura = temperatura,
                        TipoAlerta = "ALTO",
                        Mensagem = $"Temperatura atual ({temperatura}°C) está acima do limite máximo ({pref.TemperaturaMax}°C)",
                        DataHora = DateTime.Now,
                        Status = "ATIVO"
                    });
                }
                else if (pref.TemperaturaMin.HasValue && temperatura < pref.TemperaturaMin.Value)
                {
                    await _alertaService.CreateAlertaAsync(new Models.AlertaTemperatura
                    {
                        Cidade = cidade,
                        Estado = estado,
                        Temperatura = temperatura,
                        TipoAlerta = "BAIXO",
                        Mensagem = $"Temperatura atual ({temperatura}°C) está abaixo do limite mínimo ({pref.TemperaturaMin}°C)",
                        DataHora = DateTime.Now,
                        Status = "ATIVO"
                    });
                }
            }
        }
    }
}
