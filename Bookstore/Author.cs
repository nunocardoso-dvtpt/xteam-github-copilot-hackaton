namespace Bookstore;

public class Author
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly BornDate { get; set; }
    public List<string> Awards { get; set; } = [];
}
