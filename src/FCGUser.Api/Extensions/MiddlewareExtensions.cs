using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FCGUser.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection RegisterMiddlewares(this IServiceCollection services)
        {
            // Caso queira registrar middlewares customizados (ex: correlationId), faça aqui
            return services;
        }

        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler(config =>
            {
                config.Run(async context =>
                {
                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionHandler?.Error;

                    var logger = loggerFactory.CreateLogger("GlobalExceptionHandler");
                    logger.LogError(exception, "Erro não tratado capturado pelo middleware.");

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = "Ocorreu um erro inesperado. Tente novamente mais tarde."
                    });
                });
            });

            return app;
        }
    }
}
