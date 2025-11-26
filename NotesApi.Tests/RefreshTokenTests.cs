using NotesApi.Data;
using NotesApi.Models;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace NotesApi.Tests;

[TestCaseOrderer(ordererTypeName: "NotesApi.Tests.PriorityOrderer", ordererAssemblyName: "NotesApi.Tests")]
public class RefreshTokenTests : TestBed<Startup>
{
    private readonly IAuthService? _authService;
    private readonly AppDbContext? _context;
    private static bool visitedOnce = false;

    public RefreshTokenTests(ITestOutputHelper testOutputHelper, Startup fixture) : base(testOutputHelper, fixture)
    {
        _authService = fixture.GetService<IAuthService>(testOutputHelper)!;
        _context = fixture.GetService<AppDbContext>(testOutputHelper)!;

        if (!visitedOnce)
        {
            visitedOnce = true;

            _context.Users.Add(new User { Id = 1, Username = "Test User 1", PasswordHash = "Test Hash 1" });
            _context.Users.Add(new User { Id = 2, Username = "Test User 2", PasswordHash = "Test Hash 2" });
            
            _context.RefreshTokens.Add(new RefreshToken { Id = 6, Token = "6e8c282c9eaa2bd1f80e28c7d7da1eabff15fc0ea3298d26909025463eccad24", UserId = 1, CreatedByIp = "::1", UserAgent = "PostmanRuntime/7.49.1" });
            _context.RefreshTokens.Add(new RefreshToken { Id = 7, Token = "611060890231cebd43b835d1652c4d6d817ea79861a14cd4179b2aac564f9696", UserId = 1, CreatedByIp = "::1", UserAgent = "PostmanRuntime/7.49.1" });

            _context.Notes.Add(new Note { Id = 1, Title = "Note 1", Body = "Note 1" });
            _context.Notes.Add(new Note { Id = 2, Title = "Note 2", Body = "Note 2" });

            _context.SaveChanges();
        }
    }

    [Fact, TestPriority(-1)]
    public async Task TokenIssued()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiSnVuYWlkXzEiLCJ1c2VySWQiOiIxIiwiZXhwIjoxNzY2NzYyNTM0LCJpc3MiOiJOb3Rlc0FwaSIsImF1ZCI6Ik5vdGVzQXBpVXNlcnMifQ.MCV8wOk9tDp8gvr2AUHkttfLQQ0Xyd9b8oKeStaoQf8";
        var response = await _authService!.RefreshToken(token, "::1", "PostmanRuntime/7.49.1");

        Assert.NotEmpty(response.Value.token);
        Assert.NotEmpty(response.Value.refreshToken);
        Assert.NotNull(response.Value.user);
    }

    [Fact, TestPriority(0)]
    public void TokenRevoked()
    {
        var oldToken = _context?.RefreshTokens.FirstOrDefault();
        var newToken = _context?.RefreshTokens.LastOrDefault();

        Assert.NotNull(oldToken);
        Assert.True(oldToken?.IsRevoked);
        Assert.True(oldToken?.ReplacedByToken == newToken?.Token);
    }

    [Fact, TestPriority(1)]
    public void TokenRotated()
    {
        var oldToken = _context?.RefreshTokens.FirstOrDefault();
        var newToken = _context?.RefreshTokens.LastOrDefault();

        Assert.True(oldToken?.ReplacedByToken == newToken?.Token);
    }

    [Fact, TestPriority(2)]
    public async Task ExpiredTokenRejected()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiSnVuYWlkXzEiLCJ1c2VySWQiOiIxIiwiZXhwIjoxNzY2NzYyNTM0LCJpc3MiOiJOb3Rlc0FwaSIsImF1ZCI6Ik5vdGVzQXBpVXNlcnMifQ.MCV8wOk9tDp8gvr2AUHkttfLQQ0Xyd9b8oKeStaoQf8";
        await Assert.ThrowsAsync<ServiceException>(async () => await _authService!.RefreshToken(token, "::1", "PostmanRuntime/7.49.1"));
    }

    [Fact, TestPriority(3)]
    public async Task DeviceMismatch()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiSnVuYWlkXzEiLCJ1c2VySWQiOiIxIiwiZXhwIjoxNzY2NzYyOTAzLCJpc3MiOiJOb3Rlc0FwaSIsImF1ZCI6Ik5vdGVzQXBpVXNlcnMifQ.QAaroLZEMvQ12MFeu_bckkZ0GdYtkiVZvq5Yor4mrOI";
        await Assert.ThrowsAsync<ServiceException>(async () => await _authService!.RefreshToken(token, "::2", "Another_PostmanRuntime/7.49.1"));
    }
}