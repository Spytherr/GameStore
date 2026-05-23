using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace GameStore.api;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    private static readonly string[] ValidRoles = ["Buyer", "Seller"];

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        if (!ValidRoles.Contains(dto.Role))
            return ServiceResult<AuthResponseDto>.ValidationError(
                "Nieprawidłowa rola. Dozwolone wartości: Buyer, Seller.");

        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return ServiceResult<AuthResponseDto>.Conflict(
                "Użytkownik z tym adresem email już istnieje.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult<AuthResponseDto>.ValidationError(errors);
        }

        await userManager.AddToRoleAsync(user, dto.Role);

        var token = GenerateJwtToken(user, dto.Role);
        var expiration = DateTime.UtcNow.AddDays(
            configuration.GetValue<int>("Jwt:ExpirationInDays"));

        return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto(
            token, user.Email!, user.DisplayName, dto.Role, expiration));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return ServiceResult<AuthResponseDto>.ValidationError(
                "Nieprawidłowy email lub hasło.");

        var validPassword = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
            return ServiceResult<AuthResponseDto>.ValidationError(
                "Nieprawidłowy email lub hasło.");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Buyer";

        var token = GenerateJwtToken(user, role);
        var expiration = DateTime.UtcNow.AddDays(
            configuration.GetValue<int>("Jwt:ExpirationInDays"));

        return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto(
            token, user.Email!, user.DisplayName, role, expiration));
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(
                configuration.GetValue<int>("Jwt:ExpirationInDays")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
