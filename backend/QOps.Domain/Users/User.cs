namespace QOps.Domain.Users;

public enum UserRole
{
    Admin,
    Developer,
    Viewer
}

public sealed class User
{
    private User()
    {
    }

    public User(string email, string passwordHash, UserRole role = UserRole.Viewer)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        Update(email, passwordHash, role);
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("A valid user email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("User password hash is required.", nameof(passwordHash));
        }

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
