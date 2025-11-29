using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotesApi.Helpers;

public class JwtConfig(IConfiguration configuration)
{
    byte[] jwtKey = Encoding.UTF8.GetBytes(configuration[Constants.KEY_JWTKEY]!);
    string? jwtIssuer = configuration[Constants.KEY_JWTISSUER];
    string? jwtAudience = configuration[Constants.KEY_JWTAUDIENCE];

    public void GetConfig(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };
    }
}