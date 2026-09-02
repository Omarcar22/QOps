using System.IdentityModel.Tokens.Jwt;
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
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ArgumentException("Password must contain at least 8 characters.", nameof(request.Password));
        }

        var existingUser = await repository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User(request.Email, "pending", UserRole.Viewer);
        var passwordHash = passwordHasher.HashPassword(user, request.Password);
        user.Update(request.Email, passwordHash, UserRole.Viewer);

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByEmailAsync(request.Email, cancellationToken);
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
        var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = jwt["Issuer"] ?? "QOps";
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
}
