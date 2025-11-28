using NotesApi.Models;

public interface INotesRepository : IRepository<Note>
{
    Task<PaginatedList<Note>> GetAllAsync(NoteRequest noteRequest);
}