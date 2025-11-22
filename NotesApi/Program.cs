using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.MapGet("/notes", async (NotesDbContext db) => {
    return await db.Notes.ToListAsync();
});

app.MapGet("/notes/{id}", async (NotesDbContext db, int id) => {
    var note = await db.Notes.FindAsync(id);
    return note is null ? Results.NotFound() : Results.Ok(note);
});

app.MapPost("/notes", async (NotesDbContext db, Note note) => {
    if (string.IsNullOrWhiteSpace(note.Title)) {
        return Results.BadRequest(new { error = "Title is required" });
    }

    note.CreatedAt = DateTime.UtcNow;

    db.Notes.Add(note);
    await db.SaveChangesAsync();

    return Results.Created($"/notes/{note.Id}", note);
});

app.MapPut("/notes/{id}", async (NotesDbContext db, int id, Note updated) => {
    if (string.IsNullOrWhiteSpace(updated.Title)) {
        return Results.BadRequest(new { error = "Title is required" });
    }

    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    note.Title = updated.Title;
    note.Body = updated.Body;

    await db.SaveChangesAsync();

    return Results.Ok(note);
});

app.MapDelete("/notes/{id}", async (NotesDbContext db, int id) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    db.Notes.Remove(note);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.Run();