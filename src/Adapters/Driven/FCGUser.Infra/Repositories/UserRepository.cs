using Microsoft.EntityFrameworkCore;
using FCGUser.Domain.Entities;
using FCGUser.Domain.Ports;
using FCGUser.Infra.Data;

namespace FCGUser.Infra.Repositories;


public class UserRepository : IUserRepository
{
    private readonly UserDbContext _db;
    public UserRepository(UserDbContext db) => _db = db;


    public async Task AddAsync(User user, CancellationToken ct = default)
    {
    await _db.Users.AddAsync(user, ct);
    await _db.SaveChangesAsync(ct);
    }


    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    => await _db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);


    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    => await _db.Users.FindAsync(new object[]{ id }, ct);


    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
    _db.Users.Update(user);
    await _db.SaveChangesAsync(ct);
    }
    }