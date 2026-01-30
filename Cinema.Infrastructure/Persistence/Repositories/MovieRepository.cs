using Cinema.Application.Common.Interfaces.Queries;
using Cinema.Application.Common.Interfaces.Services;
using Cinema.Application.Common.Models;
using Cinema.Application.Models.Movies;
using Cinema.Application.Movies.Exceptions;
using LanguageExt;

namespace Cinema.Infrastructure.Persistence.Repositories;

public class MovieRepository(ApplicationDbContext context, IMovieApiService apiService) : IMovieQueries
{
    public async Task<Either<MovieException, PaginatedResult<ExternalMovieModel>>> SearchMoviesAsync(string query,
        int page,
        CancellationToken cancellationToken)
    {
        return await apiService.SearchMoviesAsync(query, page, cancellationToken);
    }
}