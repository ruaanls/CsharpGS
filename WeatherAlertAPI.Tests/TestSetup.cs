using Microsoft.Extensions.Options;
using Moq;
using System.Data;
using WeatherAlertAPI.Configuration;
using WeatherAlertAPI.Services;
using Dapper;

namespace WeatherAlertAPI.Tests
{
    public static class TestSetup
    {
        public static Mock<DatabaseConnection> CreateMockDatabase()
        {
            var dbSettingsMock = new Mock<IOptions<DatabaseSettings>>();
            dbSettingsMock.Setup(x => x.Value)
                         .Returns(new DatabaseSettings { ConnectionString = "mock_connection_string" });

            var dbConnectionMock = new Mock<IDbConnection>();
            var mock = new Mock<DatabaseConnection>(dbSettingsMock.Object);
            
            mock.Setup(x => x.CreateConnection())
                .Returns(dbConnectionMock.Object);

            return mock;
        }

        public static Mock<IDbConnection> GetMockConnection(Mock<DatabaseConnection> dbMock)
        {
            return Mock.Get(dbMock.Object.CreateConnection());
        }

        public static DatabaseConnection CreateRealDatabase()
        {
            var settings = Options.Create(new DatabaseSettings 
            { 
                ConnectionString = "User Id=rm557883;Password=031204;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)))" 
            });
            return new DatabaseConnection(settings);
        }        public static void SetupExecuteAsync<T>(Mock<IDbConnection> mock, T expectedParam, int returnValue = 1)
        {
            mock.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(), 
                It.Is<object>(p => JsonEquals(p, expectedParam)), 
                null as IDbTransaction, 
                null as int?))
                .ReturnsAsync(returnValue);
        }

        private static bool JsonEquals<T>(object actual, T expected)
        {
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actual);
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(expected);
            return actualJson == expectedJson;
        }
    }
}
