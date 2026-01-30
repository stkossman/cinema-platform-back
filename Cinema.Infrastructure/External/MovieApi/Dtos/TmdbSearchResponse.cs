using System.Text.Json.Serialization;

namespace Cinema.Infrastructure.External.MovieApi.Dtos;

internal class TmdbSearchResponse
{
    [JsonPropertyName("results")] public List<TmdbMovieResult> Results { get; set; } = new();

    [JsonPropertyName("page")] public int Page { get; set; }

    [JsonPropertyName("total_pages")] public int TotalPage { get; set; }

    [JsonPropertyName("total_results")] public int TotalResults { get; set; }
}

internal class TmdbMovieResult
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("original_title")] public string OriginalTitle { get; set; } = string.Empty;
    [JsonPropertyName("overview")] public string Overview { get; set; } = string.Empty;
    [JsonPropertyName("poster_path")] public string PosterPath { get; set; } = string.Empty;

    [JsonPropertyName("release_date")] public string ReleaseDate { get; set; } = string.Empty;
}