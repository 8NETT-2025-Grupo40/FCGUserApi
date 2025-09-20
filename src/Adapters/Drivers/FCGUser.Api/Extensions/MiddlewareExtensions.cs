using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;

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
                    
                    // Determinar status code baseado no tipo de exceção
                    var (statusCode, message) = exception switch
                    {
                        Microsoft.AspNetCore.Http.BadHttpRequestException => (
                            StatusCodes.Status400BadRequest,
                            "Dados da requisição inválidos."
                        ),
                        InvalidOperationException invalidOp => (
                            StatusCodes.Status400BadRequest, 
                            invalidOp.Message
                        ),
                        ArgumentException => (
                            StatusCodes.Status400BadRequest, 
                            "Argumentos inválidos fornecidos."
                        ),
                        SqlException => (
                            StatusCodes.Status503ServiceUnavailable,
                            "Serviço temporariamente indisponível. Tente novamente mais tarde."
                        ),
                        _ => (
                            StatusCodes.Status500InternalServerError,
                            "Ocorreu um erro inesperado. Tente novamente mais tarde."
                        )
                    };

                    // Log apenas erros internos (500)
                    if (statusCode == StatusCodes.Status500InternalServerError)
                    {
                        logger.LogError(exception, "Erro interno não tratado capturado pelo middleware.");
                    }
                    else if (statusCode == StatusCodes.Status503ServiceUnavailable)
                    {
                        logger.LogError(exception, "Erro de infraestrutura capturado: {StatusCode}", statusCode);
                    }
                    else
                    {
                        logger.LogWarning(exception, "Erro de validação capturado: {StatusCode}", statusCode);
                    }

                    // Configurar response
                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";

                    // Criar resposta JSON
                    var responseObject = new { Message = message };
                    var jsonResponse = JsonSerializer.Serialize(responseObject);

                    await context.Response.WriteAsync(jsonResponse);
                });
            });

            return app;
        }
    }
}
