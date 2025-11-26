using Microsoft.AspNetCore.Identity;
using NotesApi.Helpers;
using NotesApi.Models;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IStringHasher _stringHasher;

    public AuthService(IUserRepository repo, IRefreshTokenRepository refreshTokenRepo, IJwtTokenGenerator jwt, IStringHasher stringHasher)
    {
        _repo = repo;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordHasher = new PasswordHasher<User>();
        _jwt = jwt;
        _stringHasher = stringHasher;
    }

    public async Task<User?> Register(string username, string password)
    {
        var existing = await _repo.GetByUsernameAsync(username);

        if (existing != null)
            return null;

        var user = new User { Username = username };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        return await _repo.CreateAsync(user);
    }

    public async Task<(string? token, string? refreshToken, User? user)?> Login(string username, string password, string ip, string userAgent)
    {
        var user = await _repo.GetByUsernameAsync(username);

        if (user == null) 
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Success)
        {
            (string token, string refreshToken) = await CreateTokens(user, ip, userAgent);
            return (token, refreshToken, user);
        }

        return null;
    }

    public async Task<(string token, string refreshToken, User? user)?> RefreshToken(string refreshToken, string ip, string userAgent)
    {
        if (int.TryParse(_jwt.ReadUserId(refreshToken) ?? "", out int userId))
        {
            var user = await _repo.GetByIdAsync(userId) ?? throw new ServiceException(ErrorMessages.InvalidUser);
            string oldHashedToken = _stringHasher.GetSHA256Hash(refreshToken);

            if (await _refreshTokenRepo.RevokeToken(oldHashedToken, ip, userAgent))
            {
                (string token, string newRefreshToken) = await CreateTokens(user, ip, userAgent, oldHashedToken);
                return (token, newRefreshToken, user);
            }
            else
            {
                //Revoke all tokens of this user
                await _refreshTokenRepo.RevokeAllTokens(user.Id, ip);
                throw new ServiceException(ErrorMessages.AllSessionsKilled);
            }
        }

        throw new ServiceException(ErrorMessages.InvalidToken);
    }

    private async Task<(string token, string refreshToken)> CreateTokens(User user, string ip, string userAgent, string? oldToken = null)
    {
        // Generate Tokens
        string token = _jwt.GenerateToken(user, DateTime.UtcNow.AddMinutes(15));
        string refreshToken = _jwt.GenerateToken(user, DateTime.UtcNow.AddDays(30));

        // Add Hashed Refresh Token in DB
        await _refreshTokenRepo.AddTokenAsync(_stringHasher.GetSHA256Hash(refreshToken), user.Id, ip, userAgent, oldToken);

        // Return tokens
        return (token, refreshToken);
    }
}