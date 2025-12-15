using System.Net;

namespace NotesApi.Models;

public record ServiceError(string? Title = null, string? Message = null, HttpStatusCode? StatusCode = null);