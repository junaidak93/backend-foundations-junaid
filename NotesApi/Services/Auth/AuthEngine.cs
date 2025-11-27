using NotesApi.Helpers;
using NotesApi.Models;

public class AuthEngine(IJwtTokenGenerator jwt, IRefreshTokenRepository refreshTokenRepo) : IAuthEngine
{
    private readonly IJwtTokenGenerator _jwt = jwt;
    private readonly IRefreshTokenRepository _refreshTokenRepo = refreshTokenRepo;

    public string GenerateToken(User user) => 
        _jwt.GenerateToken(user, DateTime.UtcNow.AddMinutes(15));

    public string GenerateRefreshToken(User user) => 
        _jwt.GenerateToken(user, DateTime.UtcNow.AddDays(30));

    public int ExtractUserIdFromJwt(string token) => 
        int.TryParse(_jwt.ReadUserId(token) ?? "", out int userId) ? userId : 0;

    public async Task PersistToken(string hashedToken, int userId, string ip, string userAgent, string? oldHashedToken = null) => 
        await _refreshTokenRepo.AddTokenAsync(hashedToken, userId, ip, userAgent, oldHashedToken);

    public async Task<bool> TryRevokeToken(string hashedToken, string ip, string userAgent) => 
        await _refreshTokenRepo.RevokeToken(hashedToken, ip, userAgent);

    public async Task<bool> RevokeAllTokens(int userId, string ip) => 
        await _refreshTokenRepo.RevokeAllTokens(userId, ip);
}