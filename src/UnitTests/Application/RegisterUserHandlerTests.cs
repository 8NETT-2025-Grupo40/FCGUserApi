using FCGUser.Application.UseCases;
using FCGUser.Domain.Entities;
using FCGUser.Domain.Ports;
using FCGUser.Domain.Security;
using Moq;

namespace UnitTests.Application
{
    public class RegisterUserHandlerTests
    {
        [Fact]
        public async Task Handle_NewEmail_ReturnsGuid()
        {
            var repoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IBcryptPasswordHasher>();

            repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

            hasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashedpassword1234567890");

            var handler = new RegisterUserHandler(repoMock.Object, hasherMock.Object);
            var cmd = new RegisterUserCommand("test@example.com", "senha", "Test User");

            var result = await handler.Handle(cmd);

            Assert.NotEqual(Guid.Empty, result);
            repoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingEmail_Throws()
        {
            var repoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IBcryptPasswordHasher>();

            repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new User("test@example.com", "hashhashhashhashhash", "Existing User"));

            var handler = new RegisterUserHandler(repoMock.Object, hasherMock.Object);
            var cmd = new RegisterUserCommand("test@example.com", "senha", "Test User");

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(cmd));
        }
    }
}
