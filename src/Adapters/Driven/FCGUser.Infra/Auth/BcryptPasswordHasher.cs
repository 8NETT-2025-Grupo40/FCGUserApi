using FCGUser.Domain.Security;
using BCryptNet = BCrypt.Net.BCrypt;

namespace FCGUser.Infra.Auth;

public class BcryptPasswordHasher : IBcryptPasswordHasher
{
    private const int WorkFactor = 12; // custo de hash (padr�o seguro)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password n�o pode ser nula ou vazia", nameof(password));

        return BCryptNet.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password n�o pode ser nula ou vazia", nameof(password));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash n�o pode ser nulo ou vazio", nameof(passwordHash));

        return BCryptNet.Verify(password, passwordHash);
    }
}
