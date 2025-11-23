using NotesApi.Models;

public interface INotesRepository
{
    Task<List<Note>> GetAllAsync();
    Task<Note?> GetByIdAsync(int id);
    Task<Note> CreateAsync(Note note);
    Task<Note?> UpdateAsync(Note note);
    Task<bool> DeleteAsync(int id);
}