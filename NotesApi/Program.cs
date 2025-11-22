var notes = new List<NoteDto>();
var nextId = 1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.MapGet("/notes", () => Results.Ok(notes));

app.MapGet("/notes/{id}", (int id) => {
    var note = notes.FirstOrDefault(x => x.Id == id);
    return note is null ? Results.NotFound() : Results.Ok(note);
});

app.MapPost("/notes", (NoteDto note) => {
    if (string.IsNullOrWhiteSpace(note.Title)) {
        return Results.BadRequest(new { error = "Title is required" });
    }

    var newNote = note with { Id = nextId++ };
    notes.Add(newNote);

    return Results.Created($"/notes/{newNote.Id}", newNote);
});

app.MapPut("/notes/{id}", (int id, NoteDto note) => {
    if (string.IsNullOrWhiteSpace(note.Title)) {
        return Results.BadRequest(new { error = "Title is required" });
    }

    var idx = notes.FindIndex(x => x.Id == id);
    if (idx == -1) return Results.NotFound();

    notes[idx] = note with { Id = id };

    return Results.Ok(notes[idx]);
});

app.MapDelete("/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note is null) return Results.NotFound();

    notes.Remove(note);
    return Results.NoContent();
});

app.Run();