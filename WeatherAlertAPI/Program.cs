using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using WeatherAlertAPI.Services;
using WeatherAlertAPI.Models;
using WeatherAlertAPI.Middleware;
using WeatherAlertAPI.Configuration;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using WeatherAlertAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ProblemDetailsFactory, HypermediaProblemDetailsFactory>();

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
{    options.SuppressModelStateInvalidFilter = false;
    options.InvalidModelStateResponseFactory = context =>
    {        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => $"{e.Key}: {e.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid value"}")
            .ToList();

        var error = new ErrorResponse("VALIDATION_ERROR", string.Join("; ", errors));
        error.AddLink("documentation", "/docs/errors/VALIDATION_ERROR");
        error.AddLink("support", "https://weatheralert.com/support");

        var result = new BadRequestObjectResult(error);
        result.ContentTypes.Add("application/json");
        return result;
    };
});

// Configuração do Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "WeatherAlert_";
});

// Registro do serviço de cache
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather Alert API",
        Version = "v1",        Description = @"API REST para monitoramento inteligente de temperaturas e sistema de alertas meteorológicos.

🌟 Principais Funcionalidades:
- 🌡️ Monitoramento de temperaturas em tempo real
- 🔔 Sistema de notificações personalizadas
- 📊 Gestão de preferências por cidade/estado
- ⚠️ Geração automática de alertas
- 📱 Integração com serviços meteorológicos

💻 Recursos Técnicos:
- 🗃️ Oracle Database com stored procedures otimizadas
- 🔄 Padrão RESTful com HATEOAS
- 📚 Documentação interativa com Swagger/OpenAPI
- ⚡ Sistema de cache para otimização
- 🔒 Autenticação via API Key

🛠️ Tecnologias Utilizadas:
- .NET 9.0
- Oracle Database
- Entity Framework Core
- Dapper para acesso a dados
- Swagger/OpenAPI para documentação
- xUnit para testes automatizados

📋 Instruções de Uso:
1. Configure suas preferências de temperatura
2. Monitore alertas em tempo real
3. Gerencie notificações por cidade/estado
4. Consulte histórico de temperaturas",
        Contact = new OpenApiContact
        {
            Name = "Weather Alert Team",
            Email = "suporte@weatheralert.com",
            Url = new Uri("https://weatheralert.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        },
        TermsOfService = new Uri("https://weatheralert.com/terms")
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);    options.EnableAnnotations();
    
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    options.DocInclusionPredicate((docName, api) => true);
    
    options.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");
    options.DescribeAllParametersInCamelCase();
    options.UseInlineDefinitionsForEnums();
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authentication header using the Bearer scheme."
    });

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

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));
builder.Services.Configure<WeatherApiSettings>(
    builder.Configuration.GetSection("WeatherApi"));

builder.Services.AddSingleton<IDatabaseConnection, DatabaseConnection>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IPreferenciasService, PreferenciasService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IProceduresService, ProceduresService>();
builder.Services.AddHttpClient();

// Adicionar o contexto do banco de dados
builder.Services.AddDbContext<WeatherAlertContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
    app.UseStatusCodePages();
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

app.Use(async (context, next) =>
{    context.Response.OnStarting(() =>
    {
        if (!context.Response.Headers.ContainsKey("Content-Type"))
        {
            context.Response.Headers["Content-Type"] = "application/json";
        }
        return Task.CompletedTask;
    });
    await next();
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();
