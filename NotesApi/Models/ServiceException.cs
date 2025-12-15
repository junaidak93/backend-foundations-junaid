namespace NotesApi.Models;

public class ServiceException(ServiceError error, Exception? innerException = null) : Exception(error.Message, innerException)
{
    public string? Title { get; private set; } = error.Title;
    public int? StatusCode { get; private set; } = (int?)error.StatusCode;

    public ServiceException(Exception exception) : this(new ServiceError(Message: exception.Message), exception.InnerException)
    {
        
    }
}