using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using FCGUser.Application.UseCases;

namespace FCGUser.Api.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("login", async (LoginRequest req, AuthenticateUserHandler handler) =>
        {
            try
            {
                // AuthenticateUserHandler deve retornar token (string) ou null se inválido
                var token = await handler.Handle(new AuthenticateUserCommand(req.Email, req.Password));
                if (string.IsNullOrWhiteSpace(token)) return Results.Unauthorized();
                return Results.Ok(new { token });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("Login")
        .Accepts<LoginRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    public record LoginRequest(string Email, string Password);
}
