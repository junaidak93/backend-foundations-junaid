using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using NotesApi.Helpers;
using NotesApi.Models;

namespace NotesApi.ExceptionHandlers;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ServiceException serviceException = (exception as ServiceException) ?? new ServiceException(exception);
        return await DoTryHandleAsync(httpContext, serviceException, cancellationToken);
    }

    private async ValueTask<bool> DoTryHandleAsync(HttpContext httpContext, ServiceException exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = exception.StatusCode ?? (int)HttpStatusCode.InternalServerError;        
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(exception.ToProblemDetails().ToJson(), cancellationToken);
        return true;
    }
}