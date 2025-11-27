using NotesApi.Models;

public interface IAuthEngine
{
    string GenerateToken(User user);
    string GenerateRefreshToken(User user);
    int ExtractUserIdFromJwt(string token);
    Task PersistToken(string hashedToken, int userId, string ip, string userAgent, string? oldHashedToken = null);
    Task<bool> TryRevokeToken(string hashedToken, string ip, string userAgent);
    Task<bool> RevokeAllTokens(int userId, string ip);
}