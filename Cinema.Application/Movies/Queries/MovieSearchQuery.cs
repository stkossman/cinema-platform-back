using Cinema.Application.Common.Interfaces.Services;
using Cinema.Application.Common.Settings;
using Cinema.Application.Movies.Dtos;
using Cinema.Application.Movies.Exceptions;
using LanguageExt;
using MediatR;

namespace Cinema.Application.Movies.Queries;

public record MovieSearchQuery : IRequest<Either<MovieException, MovieResultSearchDto>>
{
    public required string Query { get; init; }
    public required int Page { get; init; }
}

public class MovieSearchQueryHandler(
    IMovieApiService service,
    ApplicationSettings settings)
    : IRequestHandler<MovieSearchQuery, Either<MovieException, MovieResultSearchDto>>
{
    public async Task<Either<MovieException, MovieResultSearchDto>> Handle(
        MovieSearchQuery request,
        CancellationToken cancellationToken)
    {
        var movies = await service.SearchMoviesAsync(
            request.Query,
            request.Page,
            cancellationToken);

        return movies.Match<Either<MovieException, MovieResultSearchDto>>(
            p => MovieResultSearchDto.ToDto(p, settings.ExternalApiSettings.ImageBaseUrl),
            e => e);
    }
}