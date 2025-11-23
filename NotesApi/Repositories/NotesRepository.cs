using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

public class NotesRepository : INotesRepository
{
    private readonly AppDbContext _context;

    public NotesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Note>> GetAllAsync() =>
        await _context.Notes.ToListAsync();

    public async Task<Note?> GetByIdAsync(int id) =>
        await _context.Notes.FindAsync(id);

    public async Task<Note> CreateAsync(Note note)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<Note?> UpdateAsync(Note note)
    {
        var existing = await _context.Notes.FindAsync(note.Id);
        if (existing == null) return null;

        existing.Title = note.Title;
        existing.Body = note.Body;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Notes.FindAsync(id);
        if (existing == null) return false;

        _context.Notes.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}