using Microsoft.AspNetCore.Identity;
using NotesApi.Helpers;
using NotesApi.Models;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUserRepository repo, IRefreshTokenRepository refreshTokenRepo, IJwtTokenGenerator jwt)
    {
        _repo = repo;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordHasher = new PasswordHasher<User>();
        _jwt = jwt;
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

    public async Task<(string? token, string? refreshToken, User? user)?> Login(string username, string password)
    {
        var user = await _repo.GetByUsernameAsync(username);

        if (user == null) 
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Success)
        {
            var token = _jwt.GenerateToken(user, DateTime.UtcNow.AddMinutes(15));
            var refreshToken = _jwt.GenerateToken(user, DateTime.UtcNow.AddDays(30));

            await _refreshTokenRepo.AddTokenAsync(refreshToken, user.Id);

            return (token, refreshToken, user);
        }

        return null;
    }

    public async Task<(string token, string refreshToken, User? user)?> RefreshToken(string refreshToken)
    {
        if (int.TryParse(_jwt.ReadUserId(refreshToken) ?? "", out int userId))
        {
            var user = await _repo.GetByIdAsync(userId);

            if (user is null)
                return null;

            if (await _refreshTokenRepo.DeleteAsync(refreshToken))
            {
                var token = _jwt.GenerateToken(user, DateTime.UtcNow.AddMinutes(15));
                var newRefreshToken = _jwt.GenerateToken(user, DateTime.UtcNow.AddDays(30));

                await _refreshTokenRepo.AddTokenAsync(newRefreshToken, userId);
                return (token, newRefreshToken, user);
            }
        }

        return null;
    }
}