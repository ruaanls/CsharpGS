using System;
using Microsoft.OpenApi.Models;
using WeatherAlertAPI.Services;
using WeatherAlertAPI.Configuration;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.WriteIndented = true;
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = false;
    options.InvalidModelStateResponseFactory = context =>
    {
        var result = new BadRequestObjectResult(context.ModelState);
        result.ContentTypes.Add("application/json");
        return result;
    };
});

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather Alert API",
        Version = "v1",
        Description = @"API REST para monitoramento de temperaturas e geração de alertas.

Principais funcionalidades:
- Monitoramento de temperaturas em tempo real
- Configuração de preferências de notificação
- Geração e gerenciamento de alertas",
        Contact = new OpenApiContact
        {
            Name = "Weather Alert Team",
            Email = "contato@weatheralert.com",
            Url = new Uri("https://weatheralert.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Enable annotations
    options.EnableAnnotations();

    // Group endpoints by controller
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    options.DocInclusionPredicate((docName, api) => true);

    // Add security definitions
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "Chave de API para autenticação"
    });

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    };

    options.AddSecurityRequirement(securityRequirement);
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

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure pipeline (otimizado para API REST)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger(options =>
{
    options.RouteTemplate = "api-docs/{documentName}/swagger.json";
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api-docs/v1/swagger.json", "Weather Alert API v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Weather Alert API - Documentação";
    options.DefaultModelsExpandDepth(1);
    options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.EnableDeepLinking();
    options.DisplayRequestDuration();
    options.EnableFilter();
});

// Middleware para forçar Content-Type application/json em todas as respostas
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (!context.Response.Headers.ContainsKey("Content-Type"))
        {
            context.Response.Headers.Add("Content-Type", "application/json");
        }
        return Task.CompletedTask;
    });
    await next();
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
