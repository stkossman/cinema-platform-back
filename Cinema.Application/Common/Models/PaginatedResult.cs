namespace Cinema.Application.Common.Models;

public class PaginatedResult<T>
{
    public List<T> Results { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
}