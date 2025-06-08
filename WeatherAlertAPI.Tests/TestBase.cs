using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Data;
using Dapper;
using WeatherAlertAPI.Configuration;
using WeatherAlertAPI.Services;

namespace WeatherAlertAPI.Tests
{    public class TestBase
    {
        protected readonly Mock<IDatabaseConnection> DbMock;
        protected readonly Mock<IDbConnection> ConnectionMock;
        protected readonly Mock<ILogger<AlertaService>> AlertaLoggerMock;
        protected readonly Mock<ILogger<PreferenciasService>> PreferenciasLoggerMock;
        protected readonly Mock<ILogger<WeatherService>> WeatherLoggerMock;

        public TestBase()
        {
            DbMock = TestSetup.CreateMockDatabase();
            ConnectionMock = TestSetup.GetMockConnection(DbMock);

            AlertaLoggerMock = new Mock<ILogger<AlertaService>>();
            PreferenciasLoggerMock = new Mock<ILogger<PreferenciasService>>();
            WeatherLoggerMock = new Mock<ILogger<WeatherService>>();
        }        protected void SetupMockQuery<T>(T result)
        {
            ConnectionMock
                .Setup(x => x.QueryFirstOrDefaultAsync<T>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(result);
            
            ConnectionMock
                .Setup(x => x.QueryAsync<T>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(new[] { result });
        }        protected void SetupMockExecute(int result = 1)
        {
            ConnectionMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(result);
        }        protected void SetupMockExecuteWithException(string message)
        {
            ConnectionMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ThrowsAsync(new InvalidOperationException(message));
        }
    }
}
