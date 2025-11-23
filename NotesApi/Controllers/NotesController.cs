using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;

[Authorize]
[ApiController]
[Route("[controller]")]
public class NotesController : ControllerBase
{
    private readonly INotesService _notesService;

    public NotesController(INotesService notesService)
    {
        _notesService = notesService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Note>>> GetNotes() {
        return Ok(await _notesService.GetNotesAsync());
    }

    [HttpGet("{id}")]
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