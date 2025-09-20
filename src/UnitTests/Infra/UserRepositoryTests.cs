using FCGUser.Domain.Entities;
using FCGUser.Infra.Data;
using FCGUser.Infra.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infra
{
    public class UserRepositoryTests
    {
        private UserDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new UserDbContext(options);
        }

        [Fact]
        public async Task AddGetUser_Success()
        {
            using var db = GetInMemoryDb();
            var repo = new UserRepository(db);

            var user = new User("test@example.com", new string('a', 20), "Test User");
            await repo.AddAsync(user);

            var fetched = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.Equal(user.Email, fetched!.Email);
        }

        [Fact]
        public async Task UpdateUser_Success()
        {
            using var db = GetInMemoryDb();
            var repo = new UserRepository(db);

            var user = new User("test@example.com", new string('a', 20), "Test User");
            await repo.AddAsync(user);

            user.UpdateProfile("Novo Nome");
            await repo.UpdateAsync(user);

            var fetched = await repo.GetByIdAsync(user.Id);
            Assert.Equal("Novo Nome", fetched!.Name);
        }
    }
}
