using System;
using Microsoft.OpenApi.Models;
using WeatherAlertAPI.Services;
using WeatherAlertAPI.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather Alert API",
        Version = "v1",
        Description = "API para monitoramento de temperaturas e geração de alertas",
        Contact = new OpenApiContact
        {
            Name = "Weather Alert Team",
            Email = "contato@weatheralert.com"
        }
    });
    options.EnableAnnotations();
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "WeatherAlertAPI.xml");
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.CustomSchemaIds(type => type.FullName);
});

// Configure settings
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));
builder.Services.Configure<WeatherApiSettings>(
    builder.Configuration.GetSection("WeatherApi"));

// Register services
builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IPreferenciasService, PreferenciasService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Always enable Swagger for this API
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather Alert API V1");
    c.RoutePrefix = string.Empty; // Makes Swagger UI available at the root
    c.DocumentTitle = "Documentação da API de Alertas de Temperatura";
    c.DefaultModelsExpandDepth(-1); // Hide schemas by default
    c.DisplayRequestDuration(); // Show request duration
});

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
