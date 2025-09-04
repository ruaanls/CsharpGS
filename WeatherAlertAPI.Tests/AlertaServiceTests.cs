using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Tests
{
    public class AlertaServiceTests : TestBase
    {
        private readonly AlertaService _service;

        public AlertaServiceTests()
        {
            _service = new AlertaService(DbMock.Object, AlertaLoggerMock.Object);
        }

        [Fact]
        public async Task CreateAlertaAsync_ShouldCreateAlert()
        {
            // Arrange
            var alerta = new AlertaTemperatura
            {
                Cidade = "São Paulo",
                Estado = "SP",
                TipoAlerta = "ALTO",
                Mensagem = "Temperatura muito alta",
                Temperatura = 35.5m,
                DataHora = DateTime.Now,
                Status = "ATIVO"
            };

            SetupMockExecute();

            // Act
            var result = await _service.CreateAlertaAsync(alerta);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(alerta.Cidade, result.Cidade);
            Assert.Equal(alerta.Estado, result.Estado);
            Assert.Equal(alerta.Temperatura, result.Temperatura);
        }

        [Fact]
        public async Task GetAlertaByIdAsync_ShouldReturnAlert()
        {
            // Arrange
            var expectedAlerta = new AlertaTemperatura
            {
                IdAlerta = 1,
                Cidade = "São Paulo",
                Estado = "SP",
                TipoAlerta = "ALTO",
                Temperatura = 35.5m,
                Status = "ATIVO"
            };

            SetupMockQuery(expectedAlerta);

            // Act
            var result = await _service.GetAlertaByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAlerta.IdAlerta, result.IdAlerta);
            Assert.Equal(expectedAlerta.Cidade, result.Cidade);
        }

        [Fact]
        public async Task UpdateAlertaStatusAsync_ShouldUpdateStatus()
        {
            // Arrange
            SetupMockExecute();

            // Act
            await _service.UpdateAlertaStatusAsync(1, "INATIVO");

            // Assert
            ConnectionMock.Verify(x => x.ExecuteAsync(
                    It.IsAny<CommandDefinition>()), 
                Times.Once,
                "O método Execute deveria ter sido chamado uma vez");
        }

        [Fact]
        public async Task GetAlertasAsync_ShouldReturnAlerts()
        {
            // Arrange
            var alertas = new List<AlertaTemperatura>
            {
                new AlertaTemperatura
                {
                    IdAlerta = 1,
                    Cidade = "São Paulo",
                    Estado = "SP",
                    TipoAlerta = "ALTO",
                    Temperatura = 35.5m,
                    Status = "ATIVO"
                }
            };            ConnectionMock
                .Setup(x => x.QueryAsync<AlertaTemperatura>(
                    It.IsAny<CommandDefinition>()))
                .ReturnsAsync(alertas);

            // Act
            var results = await _service.GetAlertasAsync("São Paulo", "SP");

            // Assert
            Assert.NotNull(results);
            Assert.Single(results);
        }
    }
}
