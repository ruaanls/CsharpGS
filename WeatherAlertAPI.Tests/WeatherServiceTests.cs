using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;
using WeatherAlertAPI.Configuration;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;
using System.Threading;
using System.Collections.Generic;

namespace WeatherAlertAPI.Tests
{
    public class WeatherServiceTests
    {
        private readonly Mock<IAlertaService> _alertaServiceMock;
        private readonly Mock<IPreferenciasService> _preferenciasServiceMock;
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly WeatherApiSettings _weatherApiSettings;
        private readonly WeatherService _service;

        public WeatherServiceTests()
        {
            _alertaServiceMock = new Mock<IAlertaService>();
            _preferenciasServiceMock = new Mock<IPreferenciasService>();
            _httpHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpHandlerMock.Object);
            
            _weatherApiSettings = new WeatherApiSettings
            {
                ApiKey = "test_api_key",
                BaseUrl = "http://api.weatherapi.com/v1"
            };
            var settingsMock = new Mock<IOptions<WeatherApiSettings>>();
            settingsMock.Setup(x => x.Value).Returns(_weatherApiSettings);

            _service = new WeatherService(
                _httpClient,
                settingsMock.Object,
                _alertaServiceMock.Object,
                _preferenciasServiceMock.Object);
        }

        [Fact(DisplayName = "Deve obter temperatura atual com sucesso")]
        public async Task GetCurrentTemperatureAsync_ShouldReturnTemperature()
        {
            // Arrange
            var cidade = "São Paulo";
            var estado = "SP";
            var temperatura = 25.5m;

            var response = new
            {
                current = new
                {
                    temp_c = temperatura
                }
            };

            SetupHttpMockResponse(response);

            // Act
            var result = await _service.GetCurrentTemperatureAsync(cidade, estado);

            // Assert
            Assert.Equal(temperatura, result.temperatura);
            Assert.Equal(cidade, result.cidade);
            Assert.Equal(estado, result.estado);
        }

        [Fact(DisplayName = "Deve criar alerta quando temperatura acima do limite")]
        public async Task CheckAndCreateAlertsAsync_WhenTemperatureAboveMax_ShouldCreateAlert()
        {
            // Arrange
            var preferencias = new List<PreferenciasNotificacao>
            {
                new()
                {
                    Cidade = "São Paulo",
                    Estado = "SP",
                    TemperaturaMax = 30,
                    TemperaturaMin = 15,
                    Ativo = true
                }
            };

            var response = new
            {
                current = new
                {
                    temp_c = 35.0m
                }
            };

            _preferenciasServiceMock.Setup(x => x.GetPreferenciasAsync(null, null))
                .ReturnsAsync(preferencias);

            SetupHttpMockResponse(response);

            // Act
            await _service.CheckAndCreateAlertsAsync();

            // Assert
            _alertaServiceMock.Verify(x => x.CreateAlertaAsync(
                It.Is<AlertaTemperatura>(a =>
                    a.Cidade == "São Paulo" &&
                    a.Estado == "SP" &&
                    a.Temperatura == 35.0m &&
                    a.TipoAlerta == "ALTO" &&
                    a.Status == "ATIVO")), Times.Once);
        }

        [Fact(DisplayName = "Deve criar alerta quando temperatura abaixo do limite")]
        public async Task CheckAndCreateAlertsAsync_WhenTemperatureBelowMin_ShouldCreateAlert()
        {
            // Arrange
            var preferencias = new List<PreferenciasNotificacao>
            {
                new()
                {
                    Cidade = "Curitiba",
                    Estado = "PR",
                    TemperaturaMax = 30,
                    TemperaturaMin = 15,
                    Ativo = true
                }
            };

            var response = new
            {
                current = new
                {
                    temp_c = 10.0m
                }
            };

            _preferenciasServiceMock.Setup(x => x.GetPreferenciasAsync(null, null))
                .ReturnsAsync(preferencias);

            SetupHttpMockResponse(response);

            // Act
            await _service.CheckAndCreateAlertsAsync();

            // Assert
            _alertaServiceMock.Verify(x => x.CreateAlertaAsync(
                It.Is<AlertaTemperatura>(a =>
                    a.Cidade == "Curitiba" &&
                    a.Estado == "PR" &&
                    a.Temperatura == 10.0m &&
                    a.TipoAlerta == "BAIXO" &&
                    a.Status == "ATIVO")), Times.Once);
        }

        [Fact(DisplayName = "Não deve criar alerta para preferência inativa")]
        public async Task CheckAndCreateAlertsAsync_WhenPreferenceInactive_ShouldNotCreateAlert()
        {
            // Arrange
            var preferencias = new List<PreferenciasNotificacao>
            {
                new()
                {
                    Cidade = "São Paulo",
                    Estado = "SP",
                    TemperaturaMax = 30,
                    TemperaturaMin = 15,
                    Ativo = false
                }
            };

            _preferenciasServiceMock.Setup(x => x.GetPreferenciasAsync(null, null))
                .ReturnsAsync(preferencias);

            // Act
            await _service.CheckAndCreateAlertsAsync();

            // Assert
            _alertaServiceMock.Verify(x => x.CreateAlertaAsync(It.IsAny<AlertaTemperatura>()), Times.Never);
        }

        [Fact(DisplayName = "Não deve criar alerta quando temperatura dentro dos limites")]
        public async Task CheckAndCreateAlertsAsync_WhenTemperatureWithinLimits_ShouldNotCreateAlert()
        {
            // Arrange
            var preferencias = new List<PreferenciasNotificacao>
            {
                new()
                {
                    Cidade = "São Paulo",
                    Estado = "SP",
                    TemperaturaMax = 30,
                    TemperaturaMin = 15,
                    Ativo = true
                }
            };

            var response = new
            {
                current = new
                {
                    temp_c = 20.0m
                }
            };

            _preferenciasServiceMock.Setup(x => x.GetPreferenciasAsync(null, null))
                .ReturnsAsync(preferencias);

            SetupHttpMockResponse(response);

            // Act
            await _service.CheckAndCreateAlertsAsync();

            // Assert
            _alertaServiceMock.Verify(x => x.CreateAlertaAsync(It.IsAny<AlertaTemperatura>()), Times.Never);
        }

        private void SetupHttpMockResponse(object responseContent)
        {
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JObject.FromObject(responseContent).ToString())
                });
        }
    }
}