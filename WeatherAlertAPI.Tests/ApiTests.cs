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
    public class ApiTests
    {
        private readonly Mock<IPreferenciasService> _mockPreferenciasService;
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly Mock<IAlertaService> _mockAlertaService;
        private readonly PreferenciasController _preferenciasController;
        private readonly AlertaController _alertaController;

        public ApiTests()
        {
            _mockPreferenciasService = new Mock<IPreferenciasService>();
            _mockAlertaService = new Mock<IAlertaService>();
            _mockWeatherService = new Mock<IWeatherService>();
            _preferenciasController = new PreferenciasController(_mockPreferenciasService.Object);
            _alertaController = new AlertaController(_mockAlertaService.Object, _mockWeatherService.Object);
        }

        [Fact]
        public async Task TestCriarPreferencia()
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
        public async Task TestAtualizarPreferencia()
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
        public async Task TestExcluirPreferencia()
        {
            // Arrange
            int id = 1;
            _mockPreferenciasService.Setup(service => service.DeletePreferenciaAsync(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _preferenciasController.DeletePreferencia(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetPreferencias_ReturnsList()
        {
            // Arrange
            var mockService = new Mock<IPreferenciasService>();
            var expectedPreferencias = new List<PreferenciasNotificacao>
            {
                new PreferenciasNotificacao 
                { 
                    IdPreferencia = 1,
                    Cidade = "São Paulo",
                    Estado = "SP",
                    TemperaturaMin = 15,
                    TemperaturaMax = 30
                }
            };

            mockService.Setup(s => s.GetPreferenciasAsync(null, null))
                      .ReturnsAsync(expectedPreferencias);

            var controller = new PreferenciasController(mockService.Object);

            // Act
            var result = await controller.GetPreferencias();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPrefs = Assert.IsAssignableFrom<IEnumerable<PreferenciasNotificacao>>(okResult.Value);
            Assert.Single(returnedPrefs);
            Assert.Equal("São Paulo", returnedPrefs.First().Cidade);
        }

        [Fact]
        public async Task GetPreferencia_WhenExists_ReturnsPreferencia()
        {
            // Arrange
            var mockService = new Mock<IPreferenciasService>();
            var expectedPreferencia = new PreferenciasNotificacao
            {
                IdPreferencia = 1,
                Cidade = "São Paulo",
                Estado = "SP",
                TemperaturaMin = 15,
                TemperaturaMax = 30
            };

            mockService.Setup(s => s.GetPreferenciaByIdAsync(1))
                      .ReturnsAsync(expectedPreferencia);

            var controller = new PreferenciasController(mockService.Object);

            // Act
            var result = await controller.GetPreferencia(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPref = Assert.IsType<PreferenciasNotificacao>(okResult.Value);
            Assert.Equal(1, returnedPref.IdPreferencia);
        }

        [Fact]
        public async Task CreatePreferencia_ValidData_ReturnsCreated()
        {
            // Arrange
            var mockService = new Mock<IPreferenciasService>();
            var newPreferencia = new PreferenciasNotificacao
            {
                Cidade = "São Paulo",
                Estado = "SP",
                TemperaturaMin = 15,
                TemperaturaMax = 30
            };

            mockService.Setup(s => s.CreatePreferenciaAsync(It.IsAny<PreferenciasNotificacao>()))
                      .ReturnsAsync(new PreferenciasNotificacao
                      {
                          IdPreferencia = 1,
                          Cidade = "São Paulo",
                          Estado = "SP",
                          TemperaturaMin = 15,
                          TemperaturaMax = 30
                      });

            var controller = new PreferenciasController(mockService.Object);

            // Act
            var result = await controller.CreatePreferencia(newPreferencia);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task TestCriarAlerta()
        {
            // Arrange
            var alerta = new AlertaTemperatura
            {
                Cidade = "São Paulo",
                Estado = "SP",
                Temperatura = 35.5m,
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
            Assert.Equal("São Paulo", model.Cidade);
            Assert.Equal("SP", model.Estado);
            Assert.Equal(35.5m, model.Temperatura);
            Assert.Equal("ALTO", model.TipoAlerta);
        }

        [Fact]
        public async Task TestGetAlertas()
        {
            // Arrange
            var alertas = new List<AlertaTemperatura>
            {
                new()
                {
                    IdAlerta = 1,
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Temperatura = 35.0m,
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
        public async Task UpdateAlertaStatus_ValidStatus_ReturnsNoContent()
        {
            // Arrange
            var mockAlertaService = new Mock<IAlertaService>();
            var mockWeatherService = new Mock<IWeatherService>();
            var alerta = new AlertaTemperatura
            {
                IdAlerta = 1,
                Status = "ATIVO"
            };

            mockAlertaService.Setup(s => s.GetAlertaByIdAsync(1))
                            .ReturnsAsync(alerta);

            mockAlertaService.Setup(s => s.UpdateAlertaStatusAsync(1, "INATIVO"))
                            .Returns(Task.CompletedTask);

            var controller = new AlertaController(mockAlertaService.Object, mockWeatherService.Object);

            // Act
            var result = await controller.UpdateStatus(1, "INATIVO");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
