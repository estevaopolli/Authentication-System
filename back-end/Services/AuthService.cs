namespace Services.AuthService;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Models.User;
internal sealed class AuthService(IConfiguration configuration)
{
    public string Generate(User user)
    {
        var secretKey = configuration["Jwt:Secret"];
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var handler = new JwtSecurityTokenHandler();
        var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = GenerateClaims(user),
            SigningCredentials = credentials, 
            Expires = DateTime.UtcNow.AddHours(2)};

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaims(User user)
    {
        var ci = new ClaimsIdentity();
        ci.AddClaim(new Claim(ClaimTypes.Name, user.Email));
        ci.AddClaim(new Claim(ClaimTypes.Role, user.Role));
        return ci;
    }
}