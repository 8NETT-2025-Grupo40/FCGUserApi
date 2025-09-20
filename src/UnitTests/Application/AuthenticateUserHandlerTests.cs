using FCGUser.Application.UseCases;
using FCGUser.Domain.Entities;
using FCGUser.Domain.Ports;
using FCGUser.Domain.Security;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests.Application
{
    public class AuthenticateUserHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock = new();
        private readonly Mock<IBcryptPasswordHasher> _hasherMock = new();
        private readonly Mock<IConfiguration> _configMock = new();

        public AuthenticateUserHandlerTests()
        {
            // Chave JWT deve ter pelo menos 256 bits (32 caracteres) para HS256
            _configMock.Setup(c => c["Jwt:secret"]).Returns("supersecretkey1234567890123456789012");
            _configMock.Setup(c => c["Jwt:issuer"]).Returns("http://localhost");
            _configMock.Setup(c => c["Jwt:audience"]).Returns("http://localhost");
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsToken()
        {
            var user = new User("test@example.com", "hashedpassword1234567890", "Test User");
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);
            _hasherMock.Setup(h => h.VerifyPassword("senha", user.PasswordHash)).Returns(true);

            var handler = new AuthenticateUserHandler(_repoMock.Object, _hasherMock.Object, _configMock.Object);
            var token = await handler.Handle(new AuthenticateUserCommand("test@example.com", "senha"));

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task Handle_InvalidPassword_ReturnsNull()
        {
            var user = new User("test@example.com", "hashedpassword1234567890", "Test User");
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(user);
            _hasherMock.Setup(h => h.VerifyPassword("senhaErrada", user.PasswordHash)).Returns(false);

            var handler = new AuthenticateUserHandler(_repoMock.Object, _hasherMock.Object, _configMock.Object);
            var token = await handler.Handle(new AuthenticateUserCommand("test@example.com", "senhaErrada"));

            Assert.Null(token);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User?)null);

            var handler = new AuthenticateUserHandler(_repoMock.Object, _hasherMock.Object, _configMock.Object);
            var token = await handler.Handle(new AuthenticateUserCommand("notfound@example.com", "senha"));

            Assert.Null(token);
        }
    }
}
