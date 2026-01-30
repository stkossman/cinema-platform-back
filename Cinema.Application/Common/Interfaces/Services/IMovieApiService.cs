using Cinema.Application.Common.Models;
using Cinema.Application.Models.Movies;
using Cinema.Application.Movies.Exceptions;
using LanguageExt;

namespace Cinema.Application.Common.Interfaces.Services;

public interface IMovieApiService
{
    Task<Either<MovieException, PaginatedResult<ExternalMovieModel>>> SearchMoviesAsync(string query, int page,
        CancellationToken cancellationToken);
    
    Task<Either<MovieException, ExternalMovieDetails>> GetMovieDetailsAsync(int externalId, CancellationToken cancellationToken);
}
