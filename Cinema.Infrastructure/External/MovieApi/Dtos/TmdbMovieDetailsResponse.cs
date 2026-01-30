using System.Text.Json.Serialization;

namespace Cinema.Infrastructure.External.MovieApi.Dtos;

internal class TmdbMovieDetailsResponse
{
    [JsonPropertyName("id")] 
    public int Id { get; set; }

    [JsonPropertyName("title")] 
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("poster_path")] 
    public string PosterPath { get; set; } = string.Empty;

    [JsonPropertyName("runtime")] 
    public int? Runtime { get; set; } 
    
    [JsonPropertyName("vote_average")] 
    public decimal VoteAverage { get; set; } 

    [JsonPropertyName("genres")] 
    public List<TmdbGenreDto> Genres { get; set; } = new();
}

internal class TmdbGenreDto
{
    [JsonPropertyName("id")] 
    public int Id { get; set; }

    [JsonPropertyName("name")] 
    public string Name { get; set; } = string.Empty;
}