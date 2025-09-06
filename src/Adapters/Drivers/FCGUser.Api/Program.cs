using FCGUser.Api.Extensions;
using FCGUser.Api.Endpoints;
using Microsoft.AspNetCore.Authorization;

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

app.UseCustomHealthChecks("/health");

// 🔹 Endpoints
app.MapUserEndpoints();
app.MapAuthEndpoints();

app.Run();
