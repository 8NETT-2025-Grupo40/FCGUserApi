using FCGUser.Domain.Security;
using BCryptNet = BCrypt.Net.BCrypt;
namespace FCGUser.Infra.Auth;


public class BcryptPasswordHasher : IBcryptPasswordHasher
{
    
    public string HashPassword(string password)
    {
        throw new NotImplementedException();
    }
    
    public bool VerifyPassword(string password, string passwordHash)
    {
        throw new NotImplementedException();
    }
}