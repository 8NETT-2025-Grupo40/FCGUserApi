using FCGUser.Application.UseCases;
using FCGUser.Domain.Ports;

namespace FCGUser.Api.Endpoints;

public static class UsersEndpoints
{
    // Extensão para mapear endpoints de User
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        // Registrar usuário
        group.MapPost("", async (RegisterUserRequest req, RegisterUserHandler handler) =>
        {
            try
            {
                var cmd = new RegisterUserCommand(req.Email, req.Password, req.Name);
                var id = await handler.Handle(cmd);
                return Results.Created($"/users/{id}", new { id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("RegisterUser")
        .Accepts<RegisterUserRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // Buscar usuário por id (exemplo de leitura simples via repository)
        //group.MapGet("/{id:guid}", async (Guid id, IUserRepository repo) =>
        //{
        //    var user = await repo.GetByIdAsync(id);
        //    if (user is null) return Results.NotFound();
        //    return Results.Ok(new { user.Id, user.Email, user.Name, user.CreatedAt });
        //})
        //.WithName("GetUserById")
        //.RequireAuthorization()
        //.Produces(StatusCodes.Status200OK)
        //.Produces(StatusCodes.Status404NotFound);


        // TODO: Apagar
        group.MapGet("/{id:guid}", (Guid id, IUserRepository repo) =>
            {
                return Task.FromResult(Results.Ok(id));
            })
            .WithName("GetUserById2")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // TODO: Apagar
        group.MapGet("/teste/{id:guid}", (Guid id, IUserRepository repo) =>
            {
                return Task.FromResult(Results.Ok(id));
            })
            .WithName("GetUserById1")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    // DTO usado apenas na API (binding)
    public record RegisterUserRequest(string Email, string Password, string Name);
}
