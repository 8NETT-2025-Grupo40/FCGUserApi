namespace FCGUser.Domain.Security
{
    public interface IBcryptPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
