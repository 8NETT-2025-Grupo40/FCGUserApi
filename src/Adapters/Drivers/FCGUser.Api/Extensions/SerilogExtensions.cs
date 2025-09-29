using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;

namespace FCGUser.Api.Extensions
{
    public static class SerilogExtensions
    {
        public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
        {
            // Limpa os providers padrão para forçar o uso do Serilog
            builder.Logging.ClearProviders();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}
