using NotesApi.Models;

public interface INotesService
{
    Task<List<Note>> GetNotesAsync();
    Task<PaginatedList<Note>> GetNotesAsync(NoteRequest noteRequest);
    Task<Note?> GetNoteAsync(int id);
    Task<Note> CreateNoteAsync(string title, string body);
    Task<Note?> UpdateNoteAsync(int id, string title, string body);
    Task<bool> DeleteNoteAsync(int id);
}