using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Moq;
using Xunit;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Tests
{
    public class AlertaServiceTests
    {
        private readonly Mock<DatabaseConnection> _dbMock;
        private readonly Mock<IDbConnection> _connectionMock;
        private readonly AlertaService _service;

        public AlertaServiceTests()
        {
            _dbMock = TestSetup.CreateMockDatabase();
            _connectionMock = TestSetup.GetMockConnection(_dbMock);
            _service = new AlertaService(_dbMock.Object);
        }

        [Fact(DisplayName = "Deve criar alerta com sucesso")]
        public async Task CreateAlertaAsync_ShouldCreateAlert()
        {
            // Arrange
            var alerta = new AlertaTemperatura("São Paulo", "SP", "ALTO", "Temperatura muito alta")
            {
                Temperatura = 35.5m,
                DataHora = DateTime.Now,
                Status = "ATIVO"
            };            SetupMockExecuteAsync(alerta)
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAlertaAsync(alerta);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(alerta.Cidade, result.Cidade);
            Assert.Equal(alerta.Estado, result.Estado);
            Assert.Equal(alerta.Temperatura, result.Temperatura);
            Assert.Equal(alerta.TipoAlerta, result.TipoAlerta);
            Assert.Equal(alerta.Status, result.Status);
        }

        [Fact(DisplayName = "Deve retornar alerta por ID")]
        public async Task GetAlertaByIdAsync_WhenExists_ShouldReturnAlert()
        {
            // Arrange
            var expectedAlerta = new AlertaTemperatura("Rio de Janeiro", "RJ", "BAIXO", "Temperatura muito baixa")
            {
                IdAlerta = 1,
                Temperatura = 15.0m,
                DataHora = DateTime.Now,
                Status = "ATIVO"
            };

            _connectionMock.Setup(x => x.QueryFirstOrDefaultAsync<AlertaTemperatura>(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesIdParameter(p, expectedAlerta.IdAlerta)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(expectedAlerta);

            // Act
            var result = await _service.GetAlertaByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAlerta.IdAlerta, result.IdAlerta);
            Assert.Equal(expectedAlerta.Cidade, result.Cidade);
            Assert.Equal(expectedAlerta.Estado, result.Estado);
            Assert.Equal(expectedAlerta.TipoAlerta, result.TipoAlerta);
        }

        [Fact(DisplayName = "Deve atualizar status do alerta")]
        public async Task UpdateAlertaStatusAsync_ShouldUpdateStatus()
        {
            // Arrange
            int alertaId = 1;
            string novoStatus = "INATIVO";

            _connectionMock.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesStatusUpdate(p, alertaId, novoStatus)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(1);

            // Act & Assert
            await _service.UpdateAlertaStatusAsync(alertaId, novoStatus);
        }

        [Fact(DisplayName = "Deve retornar alertas filtrados por cidade e estado")]
        public async Task GetAlertasAsync_WithFilters_ShouldReturnFilteredAlerts()
        {
            // Arrange
            var cidade = "São Paulo";
            var estado = "SP";
            var alertas = new List<AlertaTemperatura>
            {
                new(cidade, estado, "ALTO", "Temperatura muito alta")
                {
                    IdAlerta = 1,
                    Temperatura = 35.0m,
                    DataHora = DateTime.Now,
                    Status = "ATIVO"
                }
            };

            _connectionMock.Setup(x => x.QueryAsync<AlertaTemperatura>(
                It.IsAny<string>(),
                It.Is<object>(p => MatchesCidadeEstado(p, cidade, estado)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .ReturnsAsync(alertas);

            // Act
            var results = await _service.GetAlertasAsync(cidade, estado);

            // Assert
            Assert.NotNull(results);
            Assert.Single(results);
            var alerta = alertas[0];
            Assert.Equal(cidade, alerta.Cidade);
            Assert.Equal(estado, alerta.Estado);
        }

        private bool MatchesAlerta(object parameters, AlertaTemperatura alerta)
        {
            if (parameters == null) return false;
            var dict = parameters.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));

            return dict.ContainsKey("Cidade") && dict["Cidade"]?.ToString() == alerta.Cidade &&
                   dict.ContainsKey("Estado") && dict["Estado"]?.ToString() == alerta.Estado &&
                   dict.ContainsKey("Temperatura") && Equals(dict["Temperatura"], alerta.Temperatura) &&
                   dict.ContainsKey("TipoAlerta") && dict["TipoAlerta"]?.ToString() == alerta.TipoAlerta &&
                   dict.ContainsKey("Status") && dict["Status"]?.ToString() == alerta.Status;
        }

        private bool MatchesIdParameter(object parameters, int id)
        {
            if (parameters == null) return false;
            var dict = parameters.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));

            return dict.ContainsKey("id") && Equals(dict["id"], id);
        }

        private bool MatchesStatusUpdate(object parameters, int id, string status)
        {
            if (parameters == null) return false;
            var dict = parameters.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));

            return dict.ContainsKey("id") && Equals(dict["id"], id) &&
                   dict.ContainsKey("status") && dict["status"]?.ToString() == status;
        }

        private bool MatchesCidadeEstado(object parameters, string cidade, string estado)
        {
            if (parameters == null) return false;
            var dict = parameters.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));

            return dict.ContainsKey("cidade") && dict["cidade"]?.ToString() == cidade &&
                   dict.ContainsKey("estado") && dict["estado"]?.ToString() == estado;
        }

        private void SetupMockExecuteAsync<T>(T param)
        {
            _connectionMock.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => JsonEquals(p, param)),
                null as IDbTransaction,
                null as int?))
                .ReturnsAsync(1);
        }

        private bool JsonEquals<T>(object actual, T expected)
        {
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actual);
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(expected);
            return actualJson == expectedJson;
        }
    }
}
