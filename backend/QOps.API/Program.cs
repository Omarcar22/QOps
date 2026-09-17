using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QOps.Application.Environments;
using QOps.Application.Deployments;
using QOps.Application.Pipelines;
using QOps.Application.Projects;
using QOps.Application.Releases;
using QOps.Application.Users;
using QOps.Domain.Users;
using QOps.Infrastructure.Persistence;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configuredConnectionString = builder.Configuration.GetConnectionString("QOpsDatabase");
if (string.IsNullOrWhiteSpace(configuredConnectionString))
{
    configuredConnectionString = Environment.GetEnvironmentVariable("QOPS_DATABASE_CONNECTION");
}

if (string.IsNullOrWhiteSpace(configuredConnectionString))
{
    throw new InvalidOperationException("QOPS_DATABASE_CONNECTION or ConnectionStrings:QOpsDatabase must be configured.");
}

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "QOps";

if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "DevelopmentJwtKeyMustBeConfiguredInEnvironment_1234567890";
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT_KEY or Jwt:Key must be at least 32 characters long.");
}

builder.Configuration["Jwt:Key"] = jwtKey;
builder.Configuration["Jwt:Issuer"] = jwtIssuer;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanWrite", policy => policy.RequireRole("Admin", "Developer"))
    .AddPolicy("CanDelete", policy => policy.RequireRole("Admin"));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<QOpsDbContext>(options =>
    options.UseSqlServer(configuredConnectionString));

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IDeploymentRepository, DeploymentRepository>();
builder.Services.AddScoped<IDeploymentService, DeploymentService>();
builder.Services.AddScoped<IReleaseRepository, ReleaseRepository>();
builder.Services.AddScoped<IReleaseService, ReleaseService>();
builder.Services.AddScoped<IPipelineRepository, PipelineRepository>();
builder.Services.AddScoped<IPipelineService, PipelineService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<QOpsDbContext>();
    await dbContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    var demoAccounts = new[]
    {
        new
        {
            Email = (Environment.GetEnvironmentVariable("E2E_ADMIN_EMAIL") ?? "testadmin@test.com").Trim(),
            Password = Environment.GetEnvironmentVariable("E2E_ADMIN_PASSWORD") ?? "Admintest1",
            Role = UserRole.Admin,
        },
        new
        {
            Email = (Environment.GetEnvironmentVariable("E2E_DEVELOPER_EMAIL") ?? "developer-demo@qops.local").Trim(),
            Password = Environment.GetEnvironmentVariable("E2E_DEVELOPER_PASSWORD") ?? "DeveloperDemo123!",
            Role = UserRole.Developer,
        },
        new
        {
            Email = (Environment.GetEnvironmentVariable("E2E_VIEWER_EMAIL") ?? "viewer-demo@qops.local").Trim(),
            Password = Environment.GetEnvironmentVariable("E2E_VIEWER_PASSWORD") ?? "ViewerDemo123!",
            Role = UserRole.Viewer,
        },
    };

    foreach (var demoAccount in demoAccounts)
    {
        if (string.IsNullOrWhiteSpace(demoAccount.Email) || string.IsNullOrWhiteSpace(demoAccount.Password))
        {
            continue;
        }

        var existingUser = await repository.GetByEmailAsync(demoAccount.Email.ToLowerInvariant(), CancellationToken.None);

        if (existingUser is null)
        {
            var user = new User(demoAccount.Email, "placeholder", demoAccount.Role);
            var passwordHash = passwordHasher.HashPassword(user, demoAccount.Password);
            user.Update(demoAccount.Email, passwordHash, demoAccount.Role);
            user.SetActive(true);

            await repository.AddAsync(user, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
        }
        else
        {
            var passwordHash = passwordHasher.HashPassword(existingUser, demoAccount.Password);
            existingUser.Update(existingUser.Email, passwordHash, demoAccount.Role);
            existingUser.SetActive(true);
            await repository.SaveChangesAsync(CancellationToken.None);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHsts();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none';";
    await next();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}