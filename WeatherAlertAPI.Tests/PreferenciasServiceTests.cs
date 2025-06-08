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
    public class PreferenciasServiceTests : TestBase
    {
        private readonly PreferenciasService _service;

        public PreferenciasServiceTests()
        {
            _service = new PreferenciasService(DbMock.Object, PreferenciasLoggerMock.Object);
        }

        [Fact]
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

            SetupMockExecute();

            // Act
            var result = await _service.CreatePreferenciaAsync(preferencia);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(preferencia.Cidade, result.Cidade);
            Assert.Equal(preferencia.Estado, result.Estado);
        }

        [Fact]
        public async Task GetPreferenciaByIdAsync_ShouldReturnPreference()
        {
            // Arrange
            var expected = new PreferenciasNotificacao
            {
                IdPreferencia = 1,
                Cidade = "São Paulo",
                Estado = "SP",
                TemperaturaMin = 15,
                TemperaturaMax = 30,
                Ativo = true
            };

            SetupMockQuery(expected);

            // Act
            var result = await _service.GetPreferenciaByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.IdPreferencia, result.IdPreferencia);
            Assert.Equal(expected.Cidade, result.Cidade);
        }

        [Fact]
        public async Task UpdatePreferenciaAsync_ShouldUpdatePreference()
        {
            // Arrange
            var preferencia = new PreferenciasNotificacao
            {
                IdPreferencia = 1,
                Cidade = "São Paulo",
                Estado = "SP",
                TemperaturaMin = 15,
                TemperaturaMax = 30,
                Ativo = true
            };

            SetupMockExecute();

            // Act & Assert
            await _service.UpdatePreferenciaAsync(preferencia);
            // Success is indicated by no exception being thrown
        }

        [Fact]
        public async Task DeletePreferenciaAsync_ShouldDeletePreference()
        {
            // Arrange
            SetupMockExecute();

            // Act & Assert
            await _service.DeletePreferenciaAsync(1);
            // Success is indicated by no exception being thrown
        }

        [Fact]
        public async Task DeletePreferenciaAsync_WhenPreferenceDoesNotExist_ShouldThrowException()
        {
            // Arrange
            int invalidId = -1;
            ConnectionMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<CommandDefinition>()))
                .ThrowsAsync(new InvalidOperationException("Preferência não encontrada"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeletePreferenciaAsync(invalidId)
            );
            Assert.Contains("Preferência não encontrada", exception.Message);
        }
    }
}
