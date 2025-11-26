using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NotesApi.Helpers;
using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;
using Xunit.Microsoft.DependencyInjection.Abstracts;
using Xunit.Microsoft.DependencyInjection;

namespace NotesApi.Tests;

public class Startup : TestBedFixture
{
    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        //--------------------------------------------------------------------------------------------
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;        
        //--------------------------------------------------------------------------------------------

        configuration[Constants.KEY_JWTKEY] = "a-string-secret-at-least-256-bits-long";
        configuration[Constants.KEY_JWTISSUER] = "NotesApi";
        configuration[Constants.KEY_JWTAUDIENCE] = "NotesApiUsers";
        configuration[Constants.KEY_GRACEPERIODINMINUTES] = "0";
        configuration[Constants.KEY_SECRET] = "a-strong-secret-encryption-key-256-bit";
        configuration[Constants.KEY_SALT] = "a-nonce-or-mixer";

        string? connectionString = configuration.GetConnectionString(Constants.KEY_CONNECTIONSTRING);
        byte[] jwtKey = Encoding.UTF8.GetBytes(configuration[Constants.KEY_JWTKEY]);
        string? jwtIssuer = configuration[Constants.KEY_JWTISSUER];
        string? jwtAudience = configuration[Constants.KEY_JWTAUDIENCE];
        //--------------------------------------------------------------------------------------------

        // Register your dependencies here
        services
            .AddSingleton(configuration)
            .AddSingleton(new AppDbContext(options))
            .AddScoped<INotesRepository, NotesRepository>()
            .AddScoped<INotesService, NotesService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
            .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
            .AddScoped<IStringHasher, StringHasher>()

            .AddAuthentication(Constants.BEARER)
            .AddJwtBearer(Constants.BEARER, options =>
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
            });
        
        services.AddAuthorization();
    }

    protected override ValueTask DisposeAsyncCore() => new();

    protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];
}