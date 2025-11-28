using NotesApi.Models;

public static class NotesRepoExtensions
{
    public static IQueryable<Note> ApplyOrder(this IQueryable<Note> source, string? orderBy, OrderDirection orderDirection)
    {
        if (!string.IsNullOrWhiteSpace(orderBy) && orderDirection != OrderDirection.None)
        {
            source = orderBy.Trim() switch
            {
                nameof(Note.Title) when orderDirection == OrderDirection.Descending => source.OrderByDescending(x => x.Title),
                nameof(Note.Title) when orderDirection == OrderDirection.Ascending => source.OrderBy(x => x.Title),

                nameof(Note.Body) when orderDirection == OrderDirection.Descending => source.OrderByDescending(x => x.Body),
                nameof(Note.Body) when orderDirection == OrderDirection.Ascending => source.OrderBy(x => x.Body),

                _ => source
            };
        }

        return source;
    }
}