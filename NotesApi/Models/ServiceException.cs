namespace NotesApi.Models;

public class ServiceException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}