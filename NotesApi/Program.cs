using NotesApi.Data;
using NotesApi.ExceptionHandlers;
using NotesApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

DbConfig dbConfig = new(builder.Configuration);
RateLimiterConfig rateLimiterConfig = new();
JwtConfig jwtConfig = new(builder.Configuration);

// Add services to the container.
builder.Services
    .AddHttpContextAccessor()
    .AddTransient(s => s.GetService<IHttpContextAccessor>()!.HttpContext!.User!)
    .AddRateLimiter(rateLimiterConfig.GetConfig)
    .AddMemoryCache()
    .AddHostedService<TokenCleanupService>()
    .AddScoped<ICacheService, CacheService>()
    .AddDbContext<AppDbContext>(options => dbConfig.GetConfig(options))
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

builder.Services
    .AddAuthentication(Constants.BEARER)
    .AddJwtBearer(Constants.BEARER, jwtConfig.GetConfig);

builder.Services
    .Configure<ForwardedHeadersOptions>(builder.Configuration.GetSection(Constants.KEY_FORWARDEDHEADERS))
    .AddExceptionHandler<AppExceptionHandler>()
    .AddAuthorization();

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.Run();