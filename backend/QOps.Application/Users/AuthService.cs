using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QOps.Domain.Users;

namespace QOps.Application.Users;

public sealed class AuthService(
    IUserRepository repository,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration) : IAuthService
{
    public async Task<UserResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateEmail(request.Email);
        ValidatePassword(request.Password);

        var normalizedEmail = request.Email.Trim();
        var existingUser = await repository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User(normalizedEmail, "pending", UserRole.Viewer);
        var passwordHash = passwordHasher.HashPassword(user, request.Password);
        user.Update(normalizedEmail, passwordHash, UserRole.Viewer);

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateEmail(request.Email);

        var normalizedEmail = request.Email.Trim();
        var user = await repository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new AuthResponse(CreateToken(user), Map(user));
    }

    private string CreateToken(User user)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = jwt["Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = jwt["Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "QOps";
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            issuer,
            claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponse Map(User user) => new(user.Id, user.Email, user.Role, user.IsActive);

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (email.Length > 254)
        {
            throw new ArgumentException("Email is too long.", nameof(email));
        }

        try
        {
            var mailAddress = new MailAddress(email.Trim());
            if (mailAddress.Address != email.Trim())
            {
                throw new FormatException();
            }
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Password must contain at least 8 characters.", nameof(password));
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            throw new ArgumentException("Password must contain uppercase, lowercase, and numeric characters.", nameof(password));
        }
    }
}
