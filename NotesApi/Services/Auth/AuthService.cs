using NotesApi.Helpers;
using NotesApi.Models;

public class AuthService(IUserRepository userRepository, IAuthPolicy authPolicy, IAuthEngine authEngine) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuthPolicy _authPolicy = authPolicy;
    private readonly IAuthEngine _authEngine = authEngine;

    public async Task<User?> Register(string username, string password)
    {
        var existing = await _userRepository.GetByUsernameAsync(username);

        if (existing != null)
            return null;

        var user = new User { Username = username };
        user.PasswordHash = _authPolicy.HashPassword(user, password);

        return await _userRepository.CreateAsync(user);
    }

    public async Task<(string? token, string? refreshToken, User? user)?> Login(string username, string password, string ip, string userAgent)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null) 
            return null;

        if (_authPolicy.VerifyHashedPassword(user, password))
        {
            (string token, string refreshToken) = await CreateTokens(user, ip, userAgent);
            return (token, refreshToken, user);
        }

        return null;
    }

    public async Task<(string token, string refreshToken, User? user)?> RefreshToken(string refreshToken, string ip, string userAgent)
    {
        int userId = _authEngine.ExtractUserIdFromJwt(refreshToken);

        if (userId > 0)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new ServiceException(ErrorMessages.InvalidUser);
            string oldHashedToken = _authPolicy.Hash(refreshToken);

            if (await _authEngine.TryRevokeToken(oldHashedToken, ip, userAgent))
            {
                (string token, string newRefreshToken) = await CreateTokens(user, ip, userAgent, oldHashedToken);
                return (token, newRefreshToken, user);
            }
            else
            {
                //Revoke all tokens of this user
                await _authEngine.RevokeAllTokens(user.Id, ip);
                throw new ServiceException(ErrorMessages.AllSessionsKilled);
            }
        }

        throw new ServiceException(ErrorMessages.InvalidToken);
    }

    private async Task<(string token, string refreshToken)> CreateTokens(User user, string ip, string userAgent, string? oldToken = null)
    {
        // Generate Tokens
        string token = _authEngine.GenerateToken(user);
        string refreshToken = _authEngine.GenerateRefreshToken(user);

        // Add Hashed Refresh Token in DB
        await _authEngine.PersistToken(_authPolicy.Hash(refreshToken), user.Id, ip, userAgent, oldToken);

        // Return tokens
        return (token, refreshToken);
    }
}