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

    public async Task AddTokenAsync(string token, int userId)
    {
        await CreateAsync(new RefreshToken {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
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

    public async Task<bool> DeleteAsync(string refreshToken)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);
        if (existing == null) return false;

        _context.RefreshTokens.Remove(existing);
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
        if (existing == null) return null;

        existing.Token = refreshToken.Token;
        existing.UserId = refreshToken.UserId;
        existing.ExpiresAt = refreshToken.ExpiresAt;
        existing.IsRevoked = refreshToken.IsRevoked;

        await _context.SaveChangesAsync();
        return existing;
    }
}