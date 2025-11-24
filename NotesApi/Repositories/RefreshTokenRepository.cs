using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddTokenAsync(string token, int userId, string ip, string userAgent, string? oldToken = null)
    {
        List<Task> tasks = [];

        tasks.Add(CreateAsync(new RefreshToken {
            Token = token,
            UserId = userId,
            CreatedByIp = ip,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }));

        if (oldToken is not null)
        {
            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == oldToken);

            if (existing is not null)
            {
                existing.ReplacedByToken = token;
                tasks.Add(DoUpdateAsync(existing, existing));
            }
        }

        await Task.WhenAll(tasks);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.RefreshTokens.FindAsync(id);
        if (existing == null) return false;

        _context.RefreshTokens.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(IEnumerable<RefreshToken> list)
    {
        if (list is null || !list.Any()) 
            return false;

        _context.RefreshTokens.RemoveRange(list);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string refreshToken)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);
        if (existing == null) return false;

        _context.RefreshTokens.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeToken(string refreshToken, string ip, string userAgent)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && x.CreatedByIp == ip && x.UserAgent == userAgent && !x.IsRevoked);
        
        if (existing == null) 
            return false;

        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = ip;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllTokens(int userId, string ip)
    {
        var tokens = await _context.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();
        
        tokens.ForEach(token => {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ip;
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<RefreshToken>> GetAllAsync()
    {
        return await _context.RefreshTokens.ToListAsync();
    }

    public async Task<RefreshToken?> GetByIdAsync(int id)
    {
        return await _context.RefreshTokens.FindAsync(id);
    }

    public async Task<RefreshToken?> UpdateAsync(RefreshToken refreshToken)
    {
        var existing = await _context.RefreshTokens.FindAsync(refreshToken.Id);
        
        if (existing == null) 
            return null;

        return await DoUpdateAsync(existing, refreshToken);
    }

    private async Task<RefreshToken?> DoUpdateAsync(RefreshToken existing, RefreshToken refreshToken)
    {
        existing.Token = refreshToken.Token;
        existing.UserId = refreshToken.UserId;
        existing.ExpiresAt = refreshToken.ExpiresAt;
        existing.IsRevoked = refreshToken.IsRevoked;
        existing.RevokedAt = refreshToken.RevokedAt;
        existing.RevokedByIp = refreshToken.RevokedByIp;
        existing.ReplacedByToken = refreshToken.ReplacedByToken;

        await _context.SaveChangesAsync();
        return existing;
    }
}