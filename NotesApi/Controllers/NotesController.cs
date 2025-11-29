using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NotesApi.Models;

[Authorize]
[ApiController]
[Route("[controller]")]
public class NotesController(INotesService notesService) : ControllerBase
{
    private readonly INotesService _notesService = notesService;

    [HttpGet]
    [DisableRateLimiting]
    public async Task<ActionResult<PaginatedList<Note>>> GetNotes([FromQuery] NoteRequest noteRequest) {
        return Ok(await _notesService.GetNotesAsync(noteRequest));
    }

    [HttpGet("{id}")]
    [DisableRateLimiting]
    public async Task<ActionResult<Note?>> GetNotesById(int id) {
        var note = await _notesService.GetNoteAsync(id);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<Note?>> AddNote(Note note) {
        if (string.IsNullOrWhiteSpace(note.Title)) {
            return BadRequest("Title is required");
        }

        return Ok(await _notesService.CreateNoteAsync(note.Title, note.Body));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Note?>> EditNote(int id, Note updated) {
        if (string.IsNullOrWhiteSpace(updated.Title)) {
            return BadRequest("Title is required");
        }

        var note = await _notesService.UpdateNoteAsync(id, updated.Title, updated.Body);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id) {
        return await _notesService.DeleteNoteAsync(id) ? NoContent() : NotFound();
    }
}