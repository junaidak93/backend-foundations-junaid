using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NotesApi.Data;
using NotesApi.Helpers;
using NotesApi.Models;

public class NotesRepository : INotesRepository
{
    private readonly AppDbContext _context;
    private readonly int _pageSize = 20;

    public NotesRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _ = int.TryParse(configuration[Constants.KEY_PAGESIZE], out _pageSize);
    }

    public async Task<List<Note>> GetAllAsync() =>
        await _context.Notes.ToListAsync();

    public async Task<PaginatedList<Note>> GetAllAsync(NoteRequest noteRequest)
    {
        return await _context.Notes
            .Where(note => 
                string.IsNullOrWhiteSpace(noteRequest.SearchTerm) || 
                note.Title.Contains(noteRequest.SearchTerm) || 
                noteRequest.SearchTerm.Contains(note.Title)
            )
            .ApplyOrder(noteRequest.OrderBy, noteRequest.OrderDirection)
            .ToPaginatedListAsync(noteRequest.Page, _pageSize);
    }

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