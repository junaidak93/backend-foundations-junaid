using System.Text;
using System.Security.Claims;
using NotesApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace NotesApi.Helpers;

public class JwtTokenGenerator(IConfiguration config) : IJwtTokenGenerator
{
    private readonly byte[] _jwtKey = Encoding.UTF8.GetBytes(config[Constants.KEY_JWTKEY]!);
    private readonly string? _jwtIssuer = config[Constants.KEY_JWTISSUER];
    private readonly string? _jwtAudience = config[Constants.KEY_JWTAUDIENCE];

    public string GenerateToken(User user, DateTime expiry)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(Constants.CLAIM_USERID, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(_jwtKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? ReadUserId(string jwtTokenString) 
    {
        var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtTokenString);
        return jwtSecurityToken?.Claims?.FirstOrDefault(x => x.Type == Constants.CLAIM_USERID)?.Value;
    }
}