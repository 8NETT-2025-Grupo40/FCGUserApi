namespace FCGUser.Domain.Entities;


public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }


    private User() { }


    public User(string email, string passwordHash, string name)
    {
        Id = Guid.NewGuid();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Name = name;
        CreatedAt = DateTime.UtcNow;


        ValidateInvariants();
    }


    private void ValidateInvariants()
    {
        if (!Email.Contains("@")) throw new InvalidOperationException("Email inválido");
        if (PasswordHash.Length < 20) throw new InvalidOperationException("PasswordHash inválido");
    }


    public void UpdateProfile(string name)
    {
    Name = name;
    }
}