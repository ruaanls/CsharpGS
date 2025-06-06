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
            var result = await _preferenciasController.CreatePreferencia(preferencia);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var model = Assert.IsType<PreferenciasNotificacao>(createdResult.Value);
            Assert.Equal("São Paulo", model.Cidade);
            Assert.Equal("SP", model.Estado);
            Assert.Equal(15, model.TemperaturaMin);
            Assert.Equal(30, model.TemperaturaMax);
            Assert.True(model.Ativo);
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
            var result = await _alertaController.CreateAlerta(alerta);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var model = Assert.IsType<AlertaTemperatura>(createdResult.Value);
            Assert.Equal(alerta.Cidade, model.Cidade);
            Assert.Equal(alerta.Estado, model.Estado);
            Assert.Equal(alerta.Temperatura, model.Temperatura);
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
            var result = await _alertaController.GetAlertas(cidade: "", estado: "");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<AlertaTemperatura>>(okResult.Value);
            Assert.NotEmpty(model);
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
