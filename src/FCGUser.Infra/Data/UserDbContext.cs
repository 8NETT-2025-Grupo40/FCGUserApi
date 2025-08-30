using Microsoft.EntityFrameworkCore;
using FCGUser.Domain.Entities;

namespace FCGUser.Infra.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }


    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b => {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.CreatedAt).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
        });
    }
}