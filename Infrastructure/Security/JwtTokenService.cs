using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnterpriseIam.Core.Entities;
using EnterpriseIam.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseIam.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly byte[] _secretKey = Encoding.UTF8.GetBytes("YOUR_SUPER_SECRET_KEY_THAT_IS_LONG_ENOUGH_32_BYTES");

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_secretKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}