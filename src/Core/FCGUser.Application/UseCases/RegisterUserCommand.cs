using FCGUser.Domain.Entities;
using FCGUser.Domain.Ports;
using FCGUser.Domain.Security;

namespace FCGUser.Application.UseCases;

public record RegisterUserCommand(string Email, string Password, string Name);


public class RegisterUserHandler
{
    private readonly IUserRepository _repo;
    private readonly IBcryptPasswordHasher _hasher;


    public RegisterUserHandler(IUserRepository repo, IBcryptPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }


    public async Task<Guid> Handle(RegisterUserCommand cmd, CancellationToken ct = default)
    {
        var exists = await _repo.GetByEmailAsync(cmd.Email, ct);
        if (exists is not null) throw new InvalidOperationException("Email já cadastrado");


        var hash = _hasher.HashPassword(cmd.Password);
        var user = new User(cmd.Email, hash, cmd.Name);
        await _repo.AddAsync(user, ct);
        return user.Id;
    }
}