using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedExpenseTrackerApp.Database.AppDbContextModels;
using SharedExpenseTrackerApp.Domain.Models.Auth;
using SharedExpenseTrackerApp.Domain.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SharedExpenseTrackerApp.Domain.Features.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Result<LoginRespModel>> LoginAsync(LoginReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Password))
                return Result<LoginRespModel>.ValidationError("Phone number and password are required.");

            var user = await _dbContext.TblUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, ct);

            if (user is null)
                return Result<LoginRespModel>.ValidationError("Invalid phone number or password.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Result<LoginRespModel>.ValidationError("Invalid password.");

            var token = GenerateJwtToken(user);

            return Result<LoginRespModel>.Success(new LoginRespModel
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Token = token
            }, "Login successful.");
        }
        catch (Exception ex)
        {
            return Result<LoginRespModel>.SystemError($"An error occurred during login: {ex.Message}");
        }
    }

    public async Task<Result<string>> RegisterAsync(RegisterReqModel request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Result<string>.ValidationError("Full name is required.");
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return Result<string>.ValidationError("Phone number is required.");
            if (string.IsNullOrWhiteSpace(request.Password))
                return Result<string>.ValidationError("Password is required.");

            var exists = await _dbContext.TblUsers
                .AsNoTracking()
                .AnyAsync(u => u.PhoneNumber == request.PhoneNumber, ct);

            if (exists)
                return Result<string>.ValidationError("Phone number is already registered.");

            var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new TblUser
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Password = hashed,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.TblUsers.Add(user);
            await _dbContext.SaveChangesAsync(ct);

            return Result<string>.Success("Registration successful.");
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<Result<UserProfileRespModel>> GetCurrentUserAsync(long userId, CancellationToken ct = default)
    {
        try
        {
            var user = await _dbContext.TblUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user is null)
                return Result<UserProfileRespModel>.NotFound("User not found.");

            return Result<UserProfileRespModel>.Success(new UserProfileRespModel
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Result<UserProfileRespModel>.SystemError($"An error occurred: {ex.Message}");
        }
    }

    private string GenerateJwtToken(TblUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("phone", user.PhoneNumber)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
