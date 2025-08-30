using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FCGUser.Api.Extensions
{
    public static class SerilogExtensions
    {
        public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}
