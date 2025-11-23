using Microsoft.AspNetCore.Identity;
using NotesApi.Models;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository repo)
    {
        _repo = repo;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<User?> Register(string username, string password)
    {
        var existing = await _repo.GetByUsernameAsync(username);

        if (existing != null)
            return null;

        var user = new User { Username = username };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        return await _repo.CreateAsync(user);
    }

    public async Task<User?> Login(string username, string password)
    {
        var user = await _repo.GetByUsernameAsync(username);

        if (user == null) 
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success ? user : null;
    }
}