using Cinema.Application.Common.Models;
using Cinema.Application.Models.Movies;
using Cinema.Application.Movies.Exceptions;
using LanguageExt;

namespace Cinema.Application.Common.Interfaces.Queries;

public interface IMovieQueries
{
    Task<Either<MovieException, PaginatedResult<ExternalMovieModel>>> SearchMoviesAsync(string query, int page,
        CancellationToken cancellationToken);
}