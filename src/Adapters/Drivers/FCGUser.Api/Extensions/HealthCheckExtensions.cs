using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.Json;

namespace FCGUser.Api.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app, string path = "/health")
        {
            app.Map(path, builder =>
            {
                builder.Run(async context =>
                {
                    var hcService = context.RequestServices.GetRequiredService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
                    var report = await hcService.CheckHealthAsync();

                    context.Response.ContentType = "application/json";

                    var result = new
                    {
                        status = report.Status.ToString(),
                        totalDuration = report.TotalDuration.TotalMilliseconds,
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds
                        })
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                });
            });

            return app;
        }
    }
}
