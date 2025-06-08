using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherAlertAPI.Controllers;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;
using Xunit;

namespace WeatherAlertAPI.Tests
{
    public class ControllerTests
    {
        private readonly Mock<IPreferenciasService> _mockPreferenciasService;
        private readonly Mock<IAlertaService> _mockAlertaService;
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly PreferenciasController _preferenciasController;
        private readonly AlertaController _alertaController;

        public ControllerTests()
        {
            _mockPreferenciasService = new Mock<IPreferenciasService>();
            _mockAlertaService = new Mock<IAlertaService>();
            _mockWeatherService = new Mock<IWeatherService>();
            _preferenciasController = new PreferenciasController(_mockPreferenciasService.Object);
            _alertaController = new AlertaController(_mockAlertaService.Object, _mockWeatherService.Object);
        }

        [Fact]
        public async Task CreatePreferencia_ReturnsCreatedResult()
        {
            // Arrange
            var preferencia = new PreferenciasNotificacao
            {
                Cidade = "São Paulo",
                Estado = "SP",
                TemperaturaMin = 15,
                TemperaturaMax = 30,
                Ativo = true
            };

            _mockPreferenciasService.Setup(service => service.CreatePreferenciaAsync(It.IsAny<PreferenciasNotificacao>()))
                .ReturnsAsync(preferencia);

            // Act
            var result = await _preferenciasController.CreatePreferencia(preferencia);            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<HypermediaResponse<PreferenciasNotificacao>>(createdResult.Value);
            
            // Verifica os dados
            Assert.Equal("São Paulo", response.Data.Cidade);
            Assert.Equal("SP", response.Data.Estado);
            Assert.Equal(15, response.Data.TemperaturaMin);
            Assert.Equal(30, response.Data.TemperaturaMax);
            Assert.True(response.Data.Ativo);
            
            // Verifica os links HATEOAS
            Assert.Contains(response.Links, link => link.Key == "self");
            Assert.Contains(response.Links, link => link.Key == "update");
            Assert.Contains(response.Links, link => link.Key == "delete");
            Assert.Contains(response.Links, link => link.Key == "all");
            Assert.Contains(response.Links, link => link.Key == "alerts");
        }

        [Fact]
        public async Task UpdatePreferencia_ReturnsNoContent()
        {
            // Arrange
            int id = 1;
            var preferencia = new PreferenciasNotificacao
            {
                IdPreferencia = id,
                Cidade = "São Paulo",
                Estado = "SP",
                TemperaturaMin = 18,
                TemperaturaMax = 28,
                Ativo = true
            };

            _mockPreferenciasService.Setup(service => service.UpdatePreferenciaAsync(It.IsAny<PreferenciasNotificacao>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _preferenciasController.UpdatePreferencia(id, preferencia);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CreateAlerta_ReturnsCreatedResult()
        {
            // Arrange
            var alerta = new AlertaTemperatura
            {
                Cidade = "São Paulo",
                Estado = "SP",
                Temperatura = 35,
                TipoAlerta = "ALTO",
                Mensagem = "Temperatura muito alta",
                DataHora = DateTime.Now,
                Status = "ATIVO"
            };

            _mockAlertaService.Setup(service => service.CreateAlertaAsync(It.IsAny<AlertaTemperatura>()))
                .ReturnsAsync(alerta);

            // Act
            var result = await _alertaController.CreateAlerta(alerta);            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<HypermediaResponse<AlertaTemperatura>>(createdResult.Value);
            
            // Verifica os dados
            Assert.Equal(alerta.Cidade, response.Data.Cidade);
            Assert.Equal(alerta.Estado, response.Data.Estado);
            Assert.Equal(alerta.Temperatura, response.Data.Temperatura);
            
            // Verifica os links HATEOAS
            Assert.Contains(response.Links, link => link.Key == "self");
            Assert.Contains(response.Links, link => link.Key == "update_status");
            Assert.Contains(response.Links, link => link.Key == "all");
        }

        [Fact]
        public async Task GetAlertas_ReturnsOkResult()
        {
            // Arrange
            var alertas = new List<AlertaTemperatura>
            {
                new() {
                    IdAlerta = 1,
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Temperatura = 35,
                    TipoAlerta = "ALTO",
                    Mensagem = "Temperatura muito alta",
                    DataHora = DateTime.Now,
                    Status = "ATIVO"
                }
            };

            _mockAlertaService.Setup(service => service.GetAlertasAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(alertas);

            // Act
            var result = await _alertaController.GetAlertas(cidade: "", estado: "");            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<HypermediaResponse<IEnumerable<AlertaTemperatura>>>(okResult.Value);
            
            // Verifica os dados
            Assert.NotEmpty(response.Data);
            var alerta = response.Data.First();
            Assert.Equal("São Paulo", alerta.Cidade);
            Assert.Equal("SP", alerta.Estado);
            
            // Verifica os links HATEOAS
            Assert.Contains(response.Links, link => link.Key == "self");
            Assert.Contains(response.Links, link => link.Key == "check");
            Assert.Contains(response.Links, link => link.Key == "preferences");
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNoContent()
        {
            // Arrange
            int id = 1;
            string status = "INATIVO";

            _mockAlertaService.Setup(service => service.GetAlertaByIdAsync(id))
                .ReturnsAsync(new AlertaTemperatura 
                { 
                    IdAlerta = id,
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Temperatura = 25,
                    TipoAlerta = "ALTO",
                    Mensagem = "Alerta inicial",
                    DataHora = DateTime.Now,
                    Status = "ATIVO"
                });

            _mockAlertaService.Setup(service => service.UpdateAlertaStatusAsync(id, status))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _alertaController.UpdateStatus(id, status);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CheckTemperatures_ReturnsOkResult()
        {
            // Arrange
            _mockWeatherService.Setup(service => service.CheckAndCreateAlertsAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _alertaController.CheckTemperatures();

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
