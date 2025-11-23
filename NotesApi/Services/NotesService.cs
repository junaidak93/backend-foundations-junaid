using NotesApi.Models;

public class NotesService : INotesService
{
    private readonly INotesRepository _repo;
    private readonly ILogger<NotesService> _logger;

    public NotesService(INotesRepository repo, ILogger<NotesService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<Note>> GetNotesAsync() => 
        await _repo.GetAllAsync();

    public async Task<Note?> GetNoteAsync(int id) =>
        await _repo.GetByIdAsync(id);

    public async Task<Note> CreateNoteAsync(string title, string body)
    {
        var note = new Note
        {
            Title = title,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        return await _repo.CreateAsync(note);
    }

    public async Task<Note?> UpdateNoteAsync(int id, string title, string body)
    {
        var note = new Note { Id = id, Title = title, Body = body };
        return await _repo.UpdateAsync(note);
    }

    public async Task<bool> DeleteNoteAsync(int id) =>
        await _repo.DeleteAsync(id);
}