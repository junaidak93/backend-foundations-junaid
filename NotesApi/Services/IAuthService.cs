using NotesApi.Models;

public interface IAuthService
{
    Task<User?> Register(string username, string password);
    Task<(string? token, string? refreshToken, User? user)?> Login(string username, string password);
    Task<(string token, string refreshToken, User? user)?> RefreshToken(string refreshToken);
}