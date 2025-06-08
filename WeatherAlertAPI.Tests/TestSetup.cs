using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Data;
using WeatherAlertAPI.Configuration;
using WeatherAlertAPI.Services;
using Dapper;

namespace WeatherAlertAPI.Tests
{
    public static class TestSetup
    {        public static Mock<IDatabaseConnection> CreateMockDatabase()
        {
            var dbConnectionMock = new Mock<IDbConnection>();
            var mock = new Mock<IDatabaseConnection>();
            
            mock.Setup(x => x.CreateConnection())
                .Returns(dbConnectionMock.Object);

            return mock;
        }        public static Mock<IDbConnection> GetMockConnection(Mock<IDatabaseConnection> dbMock)
        {
            return Mock.Get(dbMock.Object.CreateConnection());
        }public static DatabaseConnection CreateRealDatabase()
        {
            var settings = Options.Create(new DatabaseSettings 
            { 
                ConnectionString = "User Id=rm557883;Password=031204;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)))" 
            });
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<DatabaseConnection>>();
            return new DatabaseConnection(settings, loggerMock.Object);
        }        public static void SetupExecuteAsync<T>(Mock<IDbConnection> mock, T expectedParam, int returnValue)
        {
            mock.Setup(x => x.ExecuteAsync(
                It.IsAny<CommandDefinition>()))
                .ReturnsAsync(returnValue);
        }

        public static void SetupExecuteAsync<T>(Mock<IDbConnection> mock, T expectedParam)
        {
            SetupExecuteAsync(mock, expectedParam, 1);
        }

        private static bool JsonEquals<T>(object actual, T expected)
        {
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actual);
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(expected);
            return actualJson == expectedJson;
        }
    }
}
