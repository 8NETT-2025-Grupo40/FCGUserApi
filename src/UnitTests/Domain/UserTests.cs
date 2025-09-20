using FCGUser.Domain.Entities;

namespace UnitTests.Domain
{
    public class UserTests
    {
        [Fact]
        public void CreateUser_ValidData_Success()
        {
            var user = new User("test@example.com", new string('a', 20), "Test User");

            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("Test User", user.Name);
            Assert.True(user.PasswordHash.Length >= 20);
            Assert.True(user.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void UpdateProfile_NameUpdated()
        {
            var user = new User("test@example.com", new string('a', 20), "Old Name");
            user.UpdateProfile("New Name");

            Assert.Equal("New Name", user.Name);
        }
    }
}
