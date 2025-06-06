using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using Moq;
using Xunit;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Tests
{
    public class PreferenciasServiceTests
    {
        private readonly Mock<DatabaseConnection> _dbMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly PreferenciasService _service;

        public PreferenciasServiceTests()
        {
            _dbMock = TestSetup.CreateMockDatabase();
            _connectionMock = TestSetup.GetMockConnection(_dbMock);
            _service = new PreferenciasService(_dbMock.Object);
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

            _connectionMock.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(), 
                It.Is<object>(p => MatchesPreferencia(p, preferencia)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreatePreferenciaAsync(preferencia);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(preferencia.Cidade, result.Cidade);
            Assert.Equal(preferencia.Estado, result.Estado);
            Assert.Equal(preferencia.TemperaturaMin, result.TemperaturaMin);
            Assert.Equal(preferencia.TemperaturaMax, result.TemperaturaMax);
            Assert.True(result.Ativo);
        }

        [Fact(DisplayName = "Não deve criar preferência com cidade nula")]
        public async Task CreatePreferenciaAsync_WithNullCity_ShouldThrowException()
        {
            // Arrange
            var preferencia = new PreferenciasNotificacao
            {
                Cidade = null!,
                Estado = "SP",
                TemperaturaMin = 15,
                TemperaturaMax = 30,
                Ativo = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreatePreferenciaAsync(preferencia));
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
                Ativo = true,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _connectionMock.Setup(x => x.QueryFirstOrDefaultAsync<PreferenciasNotificacao>(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesIdParameter(p, expectedPreferencia.IdPreferencia)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(expectedPreferencia);

            // Act
            var result = await _service.GetPreferenciaByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPreferencia.IdPreferencia, result.IdPreferencia);
            Assert.Equal(expectedPreferencia.Cidade, result.Cidade);
        }

        [Fact(DisplayName = "Deve retornar nulo quando preferência não existe")]
        public async Task GetPreferenciaByIdAsync_WhenNotExists_ShouldReturnNull()
        {
            // Arrange
            _connectionMock.Setup(x => x.QueryFirstOrDefaultAsync<PreferenciasNotificacao>(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesIdParameter(p, 999)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync((PreferenciasNotificacao?)null);

            // Act
            var result = await _service.GetPreferenciaByIdAsync(999);

            // Assert
            Assert.Null(result);
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

            _connectionMock.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesIdParameter(p, preferencia.IdPreferencia)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(1);

            // Act & Assert
            await _service.UpdatePreferenciaAsync(preferencia);
        }

        [Fact(DisplayName = "Deve excluir preferência com sucesso")]
        public async Task DeletePreferenciaAsync_ShouldDeletePreference()
        {
            // Arrange
            _connectionMock.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesIdParameter(p, 1)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(1);

            // Act & Assert
            await _service.DeletePreferenciaAsync(1);
        }

        [Fact(DisplayName = "Deve retornar todas as preferências quando não houver filtros")]
        public async Task GetPreferenciasAsync_WithoutFilters_ShouldReturnAllPreferences()
        {
            // Arrange
            var preferences = new List<PreferenciasNotificacao>
            {
                new() { IdPreferencia = 1, Cidade = "São Paulo", Estado = "SP" },
                new() { IdPreferencia = 2, Cidade = "Rio de Janeiro", Estado = "RJ" }
            };

            _connectionMock.Setup(x => x.QueryAsync<PreferenciasNotificacao>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(preferences);

            // Act
            var results = await _service.GetPreferenciasAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, preferences.Count);
        }

        private bool MatchesPreferencia(object parameters, PreferenciasNotificacao preferencia)
        {
            if (parameters == null) return false;
            var dict = parameters.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));

            return dict.ContainsKey("Cidade") && dict["Cidade"]?.ToString() == preferencia.Cidade &&
                   dict.ContainsKey("Estado") && dict["Estado"]?.ToString() == preferencia.Estado &&
                   dict.ContainsKey("TemperaturaMin") && Equals(dict["TemperaturaMin"], preferencia.TemperaturaMin) &&
                   dict.ContainsKey("TemperaturaMax") && Equals(dict["TemperaturaMax"], preferencia.TemperaturaMax) &&
                   dict.ContainsKey("Ativo") && Equals(dict["Ativo"], preferencia.Ativo);
        }

        private bool MatchesIdParameter(object parameters, int id)
        {
            if (parameters == null) return false;
            var dict = parameters.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));

            return dict.ContainsKey("id") && Equals(dict["id"], id);
        }
    }
}
