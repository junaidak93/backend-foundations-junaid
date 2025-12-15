using System.Net;
using NotesApi.Models;

namespace NotesApi.Helpers;

public static class ServiceErrors
{
    public static readonly ServiceError NoteNotFound = new ("Note not found", "Note not found", HttpStatusCode.NotFound);
    public static readonly ServiceError ServerError = new("Server Error", StatusCode: HttpStatusCode.InternalServerError);
    public static readonly ServiceError InvalidToken = new("Invalid Token", "Invalid Token.", HttpStatusCode.Unauthorized);
    public static readonly ServiceError InvalidUser = new("Invalid User", "User does not exist.", HttpStatusCode.BadRequest);
    public static readonly ServiceError AllSessionsKilled = new("Suspicious activity", "Suspicious activity detected. All user sessions have been killed to prevent data loss.", HttpStatusCode.Forbidden);
}