namespace FCGUser.Application.UseCases;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}