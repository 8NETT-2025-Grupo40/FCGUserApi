using FCGUser.Infra.Auth;

namespace UnitTests.Infra
{
    public class BcryptPasswordHasherTests
    {
        [Fact]
        public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            var hasher = new BcryptPasswordHasher();
            var password = "mySecretPassword123";

            // Act
            var hash = hasher.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
            Assert.True(hash.Length > 50); // BCrypt hashes são tipicamente maiores que 50 caracteres
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var hasher = new BcryptPasswordHasher();
            var password = "mySecretPassword123";
            var hash = hasher.HashPassword(password);

            // Act
            var result = hasher.VerifyPassword(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var hasher = new BcryptPasswordHasher();
            var password = "mySecretPassword123";
            var wrongPassword = "wrongPassword";
            var hash = hasher.HashPassword(password);

            // Act
            var result = hasher.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.False(result);
        }

    }
}
