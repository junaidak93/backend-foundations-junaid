using NotesApi.Models;

public class NotesService(INotesRepository repo, ILogger<NotesService> logger, ICacheService cache) : INotesService
{
    private readonly INotesRepository _repo = repo;
    private readonly ILogger<NotesService> _logger = logger;
    private readonly ICacheService _cache = cache;

    public async Task<List<Note>> GetNotesAsync() => 
        await _repo.GetAllAsync();

    public async Task<PaginatedList<Note>> GetNotesAsync(NoteRequest noteRequest)
    {
        string key = $"{nameof(NoteRequest)}.recent";

        if (_cache.TryGetValue(key, out PaginatedList<Note>? list) && list != null)
        {
            return list;
        }

        var result = await _repo.GetAllAsync(noteRequest);
        _cache.Set(key, result);
        return result;
    }

    public async Task<Note?> GetNoteAsync(int id)
    {
        string key = $"{nameof(Note)}.{id}";

        if (_cache.TryGetValue(key, out Note? note))
        {
            return note;
        }

        var result = await _repo.GetByIdAsync(id);
        _cache.Set(key, result);
        return result;
    }

    public async Task<Note> CreateNoteAsync(string title, string body)
    {
        var note = new Note
        {
            Title = title,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        Note result = await _repo.CreateAsync(note);

        _cache.Remove($"{nameof(NoteRequest)}.recent");
        return result;
    }

    public async Task<Note?> UpdateNoteAsync(int id, string title, string body)
    {
        var note = new Note { Id = id, Title = title, Body = body };
        Note? result = await _repo.UpdateAsync(note);

        _cache.Remove($"{nameof(Note)}.{id}");
        return result;
    }

    public async Task<bool> DeleteNoteAsync(int id) 
    {
        bool result = await _repo.DeleteAsync(id);

        _cache.Remove($"{nameof(Note)}.{id}");
        return result;
    }
}