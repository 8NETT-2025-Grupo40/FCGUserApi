using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace FCGUser.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FCGUser API",
                    Version = "v1",
                    Description = "Microserviço de usuários do FCG"
                });

                // 🔹 Configuração de segurança JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no campo abaixo (sem 'Bearer ')",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            return services;
        }

        public static WebApplication UseSwaggerConfiguration(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCGUser API v1");
                c.RoutePrefix = "docs"; // Swagger na raiz
            });

            return app;
        }
    }
}
