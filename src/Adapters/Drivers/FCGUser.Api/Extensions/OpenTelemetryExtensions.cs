﻿using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Extensions.AWS.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FCGUser.Api.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection SetupOpenTelemetry(this IServiceCollection services)
        {
            // Propagação de contexto:
            // - AwsXRayPropagator: entende o header X-Amzn-Trace-Id (API Gateway, etc.)
            // - TraceContext/Baggage: padrão W3C (HttpClient, serviços .NET)
            Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator([
                new AWSXRayPropagator(),
                new TraceContextPropagator(),
                new BaggagePropagator()
            ]));

            services.AddOpenTelemetry()
                // Identidade do serviço nos traces (aparece no X-Ray/Service Map)
                .ConfigureResource(r => r.AddService("fcg-user-api"))
                .WithTracing(t => t
                    // Spans automáticos para requisições ASP.NET Core
                    .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                    // Propaga trace nas chamadas entre serviços
                    .AddHttpClientInstrumentation()
                    // Spans de queries/persistência
                    .AddEntityFrameworkCoreInstrumentation()
                    // Exporta pro collector (ADOT/X-Ray)
                    .AddOtlpExporter());

            return services;
        }
    }
}
