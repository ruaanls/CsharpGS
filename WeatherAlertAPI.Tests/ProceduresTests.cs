using System;
using System.Threading.Tasks;
using WeatherAlertAPI.Services;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace WeatherAlertAPI.Tests
{
    public class ProceduresTests
    {
        private readonly DatabaseConnection _db;
        private readonly IPreferenciasService _preferenciasService;
        private readonly IAlertaService _alertaService;        public ProceduresTests()
        {
            _db = TestSetup.CreateRealDatabase();
            _preferenciasService = new PreferenciasService(_db);
            _alertaService = new AlertaService(_db);
        }

        [Fact]
        public async Task TestSP_INSERIR_PREFERENCIA()
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

            // Act
            var result = await _preferenciasService.CreatePreferenciaAsync(preferencia);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IdPreferencia > 0);
            Assert.Equal("São Paulo", result.Cidade);
        }

        [Fact]        public async Task TestSP_ATUALIZAR_PREFERENCIA()
        {
            // Arrange
            var preferencia = new PreferenciasNotificacao
            {
                IdPreferencia = 1,
                Cidade = "Rio de Janeiro",
                Estado = "RJ",
                TemperaturaMin = 20,
                TemperaturaMax = 35,
                Ativo = true
            };

            // Act & Assert
            await _preferenciasService.UpdatePreferenciaAsync(preferencia);
            var updated = await _preferenciasService.GetPreferenciaByIdAsync(1);
            Assert.NotNull(updated);
            Assert.Equal("Rio de Janeiro", updated.Cidade);
        }

        [Fact]
        public async Task TestSP_INSERIR_ALERTA()
        {
            // Arrange
            var alerta = new AlertaTemperatura
            {
                Cidade = "São Paulo",
                Estado = "SP",
                Temperatura = 35.5m,
                TipoAlerta = "ALTO",
                Mensagem = "Temperatura muito alta",
                Status = "ATIVO"
            };

            // Act
            var result = await _alertaService.CreateAlertaAsync(alerta);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IdAlerta > 0);
            Assert.Equal("São Paulo", result.Cidade);
        }

        [Fact]
        public async Task TestSP_ATUALIZAR_STATUS_ALERTA()
        {
            // Arrange
            int alertaId = 1;
            string novoStatus = "INATIVO";

            // Act
            await _alertaService.UpdateAlertaStatusAsync(alertaId, novoStatus);            // Assert
            var updated = await _alertaService.GetAlertaByIdAsync(alertaId);
            Assert.NotNull(updated);
            Assert.Equal("INATIVO", updated.Status);
        }
    }
}