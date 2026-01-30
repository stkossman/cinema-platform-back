namespace Cinema.Application.Models.Movies;

public class ExternalMovieModel
{
    public required int ExternalId { get; init; }
    public required string Title { get; init; }
    public required string OriginalTitle { get; init; }
    public required string Overview { get; init; }
    public required string PosterPath { get; init; }
    public required DateTime? ReleaseDate { get; init; }
}

public class ExternalMovieDetails
{
    public required int ExternalId { get; init; }
    public required string Title { get; init; }
    public required string? PosterUrl { get; init; }
    public required decimal Rating { get; init; }
    public required int DurationMinutes { get; init; }
    public required List<ExternalGenre> Genres { get; init; }
}

public class ExternalGenre
{
    public required int ExternalId { get; init; }
    public required string Name { get; init; }
}