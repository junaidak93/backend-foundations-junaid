namespace NotesApi.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string? Token { get; set; }
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
    public DateTime RevokedAt { get; set; } = DateTime.MaxValue;
    public string RevokedByIp { get; set; } = "";
    public string? ReplacedByToken { get; set; }
}
