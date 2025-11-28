public record PaginatedList<T>(IEnumerable<T> Items, int Count, int PageNumber, int PageSize)
{
    public int TotalPages { get; private set; } = (int)Math.Ceiling(Count / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}