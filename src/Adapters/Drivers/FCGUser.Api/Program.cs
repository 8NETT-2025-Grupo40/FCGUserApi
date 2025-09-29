using FCGUser.Api.Extensions;
using FCGUser.Api.Endpoints;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Extensions.AWS.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Logging
builder.ConfigureSerilog();

// 🔹 Services
builder.Services.RegisterServices();
builder.Services.RegisterMiddlewares();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddDbContextConfiguration(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.SetupOpenTelemetry();

// 🔹 Build app
var app = builder.Build();

// 🔹 Middlewares
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker" || app.Environment.IsProduction())
{
    app.UseSwaggerConfiguration();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCustomExceptionHandler(app.Services.GetRequiredService<ILoggerFactory>());

// 🔹 Endpoints
app.MapHealthCheckEndpoints();
app.MapUserEndpoints();
app.MapAuthEndpoints();

app.Run();

// Necessário para testes de integração
public partial class Program { }