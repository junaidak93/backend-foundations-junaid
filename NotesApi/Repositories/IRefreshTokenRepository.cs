using NotesApi.Models;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task AddTokenAsync(string token, int userId);
    Task<bool> DeleteAsync(string refreshToken);
}