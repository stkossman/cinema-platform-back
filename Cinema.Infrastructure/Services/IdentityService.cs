using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Entities;
using Cinema.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cinema.Infrastructure.Services;

public class IdentityService(
    UserManager<User> userManager,
    IConfiguration configuration) : IIdentityService
{
    public async Task<Result<Guid>> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<Guid>(new Error("Identity.RegisterFailed", errors));
        }

        await userManager.AddToRoleAsync(user, "User");

        return Result.Success(user.Id);
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure<string>(new Error("Identity.LoginFailed", "Invalid email or password."));

        var checkPassword = await userManager.CheckPasswordAsync(user, password);
        if (!checkPassword)
            return Result.Failure<string>(new Error("Identity.LoginFailed", "Invalid email or password."));

        var roles = await userManager.GetRolesAsync(user);

        var token = GenerateJwtToken(user, roles);
        return Result.Success(token);
    }

    private string GenerateJwtToken(User user, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("firstName", user.FirstName ?? ""),
            new("lastName", user.LastName ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"]!)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}