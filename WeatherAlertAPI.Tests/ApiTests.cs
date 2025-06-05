using Xunit;
using Moq;
using WeatherAlertAPI.Controllers;
using WeatherAlertAPI.Services;
using WeatherAlertAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WeatherAlertAPI.Tests
{
    public class ApiTests
    {
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
                    TemperaturaMinima = 15,
                    TemperaturaMaxima = 30
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
                TemperaturaMinima = 15,
                TemperaturaMaxima = 30
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
                TemperaturaMinima = 15,
                TemperaturaMaxima = 30
            };

            mockService.Setup(s => s.CreatePreferenciaAsync(It.IsAny<PreferenciasNotificacao>()))
                      .ReturnsAsync(new PreferenciasNotificacao
                      {
                          IdPreferencia = 1,
                          Cidade = "São Paulo",
                          Estado = "SP",
                          TemperaturaMinima = 15,
                          TemperaturaMaxima = 30
                      });

            var controller = new PreferenciasController(mockService.Object);

            // Act
            var result = await controller.CreatePreferencia(newPreferencia);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task GetAlertas_ReturnsList()
        {
            // Arrange
            var mockAlertaService = new Mock<IAlertaService>();
            var mockWeatherService = new Mock<IWeatherService>();
            var expectedAlertas = new List<AlertaTemperatura>
            {
                new AlertaTemperatura
                {
                    IdAlerta = 1,
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Temperatura = 32,
                    TipoAlerta = "TEMPERATURA_ALTA",
                    Status = "ATIVO"
                }
            };

            mockAlertaService.Setup(s => s.GetAlertasAsync(null, null))
                            .ReturnsAsync(expectedAlertas);

            var controller = new AlertaController(mockAlertaService.Object, mockWeatherService.Object);

            // Act
            var result = await controller.GetAlertas();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAlertas = Assert.IsAssignableFrom<IEnumerable<AlertaTemperatura>>(okResult.Value);
            Assert.Single(returnedAlertas);
            Assert.Equal("TEMPERATURA_ALTA", returnedAlertas.First().TipoAlerta);
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
