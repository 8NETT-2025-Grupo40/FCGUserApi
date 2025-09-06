using FCGUser.Domain.Entities;
using FCGUser.Domain.Ports;
using FCGUser.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FCGUser.Application.UseCases
{
    public record AuthenticateUserCommand(string Email, string Password);

    public class AuthenticateUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IBcryptPasswordHasher _passwordHasher;
        private readonly IConfiguration _config;

        public AuthenticateUserHandler(
            IUserRepository userRepository,
            IBcryptPasswordHasher passwordHasher,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        public async Task<string?> Handle(AuthenticateUserCommand command)
        {
            // 1. Buscar usuário por email
            var user = await _userRepository.GetByEmailAsync(command.Email);
            if (user is null) return null;

            // 2. Validar senha
            if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
                return null;

            // 3. Gerar JWT
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
