namespace WeatherAlertAPI.Configuration
{
    public class AppSettings
    {
        public required DatabaseSettings Database { get; set; }
        public required WeatherApiSettings WeatherApi { get; set; }
    }

    public class DatabaseSettings
    {
        public required string ConnectionString { get; set; }
    }

    public class WeatherApiSettings
    {
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
    }
}
