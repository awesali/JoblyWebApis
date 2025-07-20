using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JoblyWebApi.Data.Models;
using Microsoft.IdentityModel.Tokens;

public class JwtService
{
    private readonly string _key;
    public JwtService(string key) { _key = key; }

    public string GenerateToken(UserLogin user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Email.ToString()),
            new Claim(ClaimTypes.Email, user.Password)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
