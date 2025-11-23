using Microsoft.EntityFrameworkCore;
using NotesApi.Data;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services
    .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
    .AddScoped<INotesRepository, NotesRepository>()
    .AddScoped<INotesService, NotesService>()
    .AddControllers();

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();

app.Run();