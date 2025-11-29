using Microsoft.EntityFrameworkCore;
using NotesApi.Helpers;

public class DbConfig(IConfiguration configuration)
{
    private string? connectionString = configuration.GetConnectionString(Constants.KEY_CONNECTIONSTRING);
    
    public DbContextOptionsBuilder GetConfig(DbContextOptionsBuilder options)
    {
        return options.UseNpgsql(connectionString);
    }
}