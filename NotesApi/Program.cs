using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApi.Helpers;
using Microsoft.AspNetCore.Identity;
using NotesApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString(Constants.KEY_CONNECTIONSTRING);
byte[] jwtKey = Encoding.UTF8.GetBytes(builder.Configuration[Constants.KEY_JWTKEY]!);
string? jwtIssuer = builder.Configuration[Constants.KEY_JWTISSUER];
string? jwtAudience = builder.Configuration[Constants.KEY_JWTAUDIENCE];

// Add services to the container.
builder.Services
    .AddHttpContextAccessor()
    .AddTransient(s => s.GetService<IHttpContextAccessor>()!.HttpContext!.User!)
    .AddMemoryCache()
    .AddHostedService<TokenCleanupService>()
    .AddScoped<ICacheService, CacheService>()
    .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
    .AddScoped<INotesRepository, NotesRepository>()
    .AddScoped<INotesService, NotesService>()
    .AddScoped<IAuthPolicy, AuthPolicy>()
    .AddScoped<IAuthEngine, AuthEngine>()
    .AddScoped<IAuthService, AuthService>()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
    .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
    .AddScoped<IStringHasher, StringHasher>()
    .AddControllers();

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 5; // Allow 5 requests
        options.Window = TimeSpan.FromSeconds(10); // Within a 10-second window
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2; // Queue up to 2 additional requests
    });

    limiterOptions.AddConcurrencyLimiter("concurrent", options =>
    {
        options.PermitLimit = 3; // Allow 3 concurrent requests
        options.QueueProcessingOrder = QueueProcessingOrder.NewestFirst;
        options.QueueLimit = 0; // Don't queue requests
    });

    // You can also define global policies or policies based on specific criteria (e.g., user, IP)
    // limiterOptions.AddPolicy("userPolicy", httpContext =>
    // {
    //     // Logic to determine a policy based on the user or other request details
    //     return RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity.Name, _ =>
    //         new FixedWindowRateLimiterOptions
    //         {
    //             PermitLimit = 10,
    //             Window = TimeSpan.FromMinutes(1)
    //         });
    // });

    limiterOptions.OnRejected = (context, cancellationToken) =>
    {
        // Handle rejected requests (e.g., set status code 429 Too Many Requests)
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

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