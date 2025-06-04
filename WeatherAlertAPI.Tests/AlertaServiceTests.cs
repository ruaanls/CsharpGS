using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Tests
{
    public class AlertaServiceTests
    {
        private readonly Mock<DatabaseConnection> _dbMock;

        public AlertaServiceTests()
        {
            _dbMock = new Mock<DatabaseConnection>();
        }

        [Fact(DisplayName = "Deve criar alerta com sucesso")]
        public async Task CreateAlertaAsync_ShouldCreateAlert()
        {
            // Arrange
            var alerta = new AlertaTemperatura
            {
                Cidade = "São Paulo",
                Estado = "SP",
                Temperatura = 32.5m,
                TipoAlerta = "ALTO",
                Mensagem = "Temperatura muito alta",
                DataHora = DateTime.Now,
                Status = "ATIVO"
            };

            var service = new AlertaService(_dbMock.Object);

            // Act
            var result = await service.CreateAlertaAsync(alerta);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(alerta.Cidade, result.Cidade);
            Assert.Equal(alerta.Estado, result.Estado);
            Assert.Equal(alerta.Temperatura, result.Temperatura);
            Assert.Equal(alerta.TipoAlerta, result.TipoAlerta);
        }

        [Fact(DisplayName = "Deve retornar alerta por ID")]
        public async Task GetAlertaByIdAsync_WhenExists_ShouldReturnAlert()
        {
            // Arrange
            var expectedAlerta = new AlertaTemperatura
            {
                IdAlerta = 1,
                Cidade = "Rio de Janeiro",
                Estado = "RJ",
                Temperatura = 38.0m,
                TipoAlerta = "ALTO",
                Status = "ATIVO"
            };

            var service = new AlertaService(_dbMock.Object);

            // Act
            var result = await service.GetAlertaByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAlerta.IdAlerta, result.IdAlerta);
        }

        [Fact(DisplayName = "Deve atualizar status do alerta")]
        public async Task UpdateAlertaStatusAsync_ShouldUpdateStatus()
        {
            // Arrange
            var service = new AlertaService(_dbMock.Object);
            var alertaId = 1;
            var novoStatus = "INATIVO";

            // Act & Assert
            await service.UpdateAlertaStatusAsync(alertaId, novoStatus);
            // Se não lançar exceção, o teste passa
        }

        [Fact(DisplayName = "Deve retornar alertas filtrados por cidade e estado")]
        public async Task GetAlertasAsync_WithFilters_ShouldReturnFilteredAlerts()
        {
            // Arrange
            var cidade = "Curitiba";
            var estado = "PR";
            var service = new AlertaService(_dbMock.Object);

            // Act
            var result = await service.GetAlertasAsync(cidade, estado);

            // Assert
            Assert.NotNull(result);
        }
    }
}
