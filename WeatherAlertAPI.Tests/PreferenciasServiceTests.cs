using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Tests
{
    public class PreferenciasServiceTests
    {
        private readonly Mock<DatabaseConnection> _dbMock;

        public PreferenciasServiceTests()
        {
            _dbMock = new Mock<DatabaseConnection>();
        }

        [Fact(DisplayName = "Deve criar preferência com sucesso")]
        public async Task CreatePreferenciaAsync_ShouldCreatePreference()
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

            var service = new PreferenciasService(_dbMock.Object);

            // Act
            var result = await service.CreatePreferenciaAsync(preferencia);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(preferencia.Cidade, result.Cidade);
            Assert.Equal(preferencia.Estado, result.Estado);
            Assert.Equal(preferencia.TemperaturaMin, result.TemperaturaMin);
            Assert.Equal(preferencia.TemperaturaMax, result.TemperaturaMax);
            Assert.True(result.Ativo);
        }

        [Fact(DisplayName = "Deve retornar preferência por ID")]
        public async Task GetPreferenciaByIdAsync_WhenExists_ShouldReturnPreference()
        {
            // Arrange
            var expectedPreferencia = new PreferenciasNotificacao
            {
                IdPreferencia = 1,
                Cidade = "Rio de Janeiro",
                Estado = "RJ",
                TemperaturaMin = 20,
                TemperaturaMax = 35,
                Ativo = true
            };

            var service = new PreferenciasService(_dbMock.Object);

            // Act
            var result = await service.GetPreferenciaByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPreferencia.IdPreferencia, result.IdPreferencia);
        }

        [Fact(DisplayName = "Deve atualizar preferência com sucesso")]
        public async Task UpdatePreferenciaAsync_ShouldUpdatePreference()
        {
            // Arrange
            var preferencia = new PreferenciasNotificacao
            {
                IdPreferencia = 1,
                Cidade = "Curitiba",
                Estado = "PR",
                TemperaturaMin = 10,
                TemperaturaMax = 25,
                Ativo = true
            };

            var service = new PreferenciasService(_dbMock.Object);

            // Act & Assert
            await service.UpdatePreferenciaAsync(preferencia);
            // Se não lançar exceção, o teste passa
        }

        [Fact(DisplayName = "Deve excluir preferência com sucesso")]
        public async Task DeletePreferenciaAsync_ShouldDeletePreference()
        {
            // Arrange
            var service = new PreferenciasService(_dbMock.Object);

            // Act & Assert
            await service.DeletePreferenciaAsync(1);
            // Se não lançar exceção, o teste passa
        }
    }
}
