using System.Text;

public record NoteRequest(string? SearchTerm = null, int Page = 1, string? OrderBy = null, OrderDirection OrderDirection = OrderDirection.None)
{
    public override string ToString()
    {
        StringBuilder builder = new();
        
        builder.Append(SearchTerm);
        builder.Append(Page);
        builder.Append(OrderBy);
        builder.Append(OrderDirection);

        return builder.ToString();
    }
}