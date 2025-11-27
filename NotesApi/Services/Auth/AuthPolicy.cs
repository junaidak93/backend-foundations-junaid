using Microsoft.AspNetCore.Identity;
using NotesApi.Models;

public class AuthPolicy(IStringHasher stringHasher) : IAuthPolicy
{
    private readonly IStringHasher _stringHasher = stringHasher;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password) => _passwordHasher.HashPassword(user, password);

    public bool VerifyHashedPassword(User user, string password) => 
        _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;

    public string Hash(string input) => _stringHasher.GetSHA256Hash(input);
}