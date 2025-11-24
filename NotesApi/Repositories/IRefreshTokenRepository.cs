using NotesApi.Models;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task AddTokenAsync(string token, int userId, string ip, string userAgent, string? oldToken = null);
    Task<bool> DeleteAsync(string refreshToken);
    Task<bool> RevokeToken(string refreshToken, string ip, string userAgent);
    Task<bool> RevokeAllTokens(int userId, string ip);
    Task<bool> DeleteAsync(IEnumerable<RefreshToken> list);
}