using NotesApi.Models;

namespace NotesApi.Helpers;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, DateTime expiry);
    string? ReadUserId(string jwtTokenString);
}