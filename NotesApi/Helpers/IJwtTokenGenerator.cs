using NotesApi.Models;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}