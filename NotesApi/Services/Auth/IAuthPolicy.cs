using NotesApi.Models;

public interface IAuthPolicy
{
    string HashPassword(User user, string password);
    bool VerifyHashedPassword(User user, string password);
    string Hash(string input);
}