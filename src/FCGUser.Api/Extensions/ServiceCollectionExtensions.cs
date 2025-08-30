using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCGUser.Domain.Ports;
using FCGUser.Infra.Data;
using FCGUser.Infra.Repositories;
using FCGUser.Infra.Auth;
using FCGUser.Domain.Security;
using FCGUser.Application.UseCases;

namespace FCGUser.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBcryptPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<RegisterUserHandler>();
            services.AddScoped<AuthenticateUserHandler>();
            return services;
        }

        public static IServiceCollection AddDbContextConfiguration(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
    }
}
