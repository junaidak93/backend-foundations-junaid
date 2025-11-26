using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString(Constants.KEY_CONNECTIONSTRING);
byte[] jwtKey = Encoding.UTF8.GetBytes(builder.Configuration[Constants.KEY_JWTKEY]!);
string? jwtIssuer = builder.Configuration[Constants.KEY_JWTISSUER];
string? jwtAudience = builder.Configuration[Constants.KEY_JWTAUDIENCE];

// Add services to the container.
builder.Services
    .AddHostedService<TokenCleanupService>()
    .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
    .AddScoped<INotesRepository, NotesRepository>()
    .AddScoped<INotesService, NotesService>()
    .AddScoped<IAuthService, AuthService>()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
    .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
    .AddScoped<IStringHasher, StringHasher>()
    .AddControllers();

builder.Services.AddAuthentication(Constants.BEARER)
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

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();