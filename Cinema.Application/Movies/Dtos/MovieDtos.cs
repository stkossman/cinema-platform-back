using Cinema.Application.Common.Models;
using Cinema.Application.Models.Movies;
using Cinema.Domain.Entities;

namespace Cinema.Application.Movies.Dtos;

public record MovieDto(
    Guid Id,
    string Title,
    int ExternalId,
    int DurationMinutes,
    decimal Rating,
    string? ImgUrl,
    string? VideoUrl)
{
    public static MovieDto FromDomainModel(Movie movie, string imgBaseUrl)
    {
        return new MovieDto(
            movie.Id.Value,
            movie.Title,
            movie.ExternalId,
            movie.DurationMinutes,
            movie.Rating,
            string.IsNullOrEmpty(movie.ImgUrl)
                ? null
                : $"{imgBaseUrl}{movie.ImgUrl}",
            movie.VideoUrl
        );
    }
}

public record MovieResultSearchDto(
    int Page,
    int TotalPage,
    int TotalResults,
    IReadOnlyList<MovieSearchDto> Results)
{
    public static MovieResultSearchDto ToDto(PaginatedResult<ExternalMovieModel> result, string imgBaseUrl)
    {
        return new MovieResultSearchDto(
            result.Page,
            result.TotalPages,
            result.TotalResults,
            result.Results.Select(x => MovieSearchDto.FromDomainModel(x, imgBaseUrl)).ToList()
        );
    }
}

public record MovieSearchDto(
    int ExternalId,
    string Title,
    string OriginalTitle,
    string Overview,
    string PosterPath,
    DateTime? ReleaseDate)
{
    public static MovieSearchDto FromDomainModel(ExternalMovieModel model, string imgBaseUrl)
    {
        return new MovieSearchDto(
            model.ExternalId,
            model.Title,
            model.OriginalTitle,
            model.Overview,
            string.IsNullOrEmpty(model.PosterPath)
                ? ""
                : $"{imgBaseUrl}{model.PosterPath}",
            model.ReleaseDate
        );
    }
}