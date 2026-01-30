using Cinema.Domain.Common;
using Cinema.Domain.Entities;

namespace Cinema.Application.Movies.Exceptions;

public abstract class MovieException(
    EntityId<Movie> movieId,
    string message,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public EntityId<Movie> MovieId { get; } = movieId;
}

public class MovieNotFoundException(EntityId<Movie> movieId, int externalId)
    : MovieException(movieId, $"Movie with ID '{externalId}' was not found.");

public class MovieApiNetworkException(EntityId<Movie> movieId, string message, Exception? inner = null)
    : MovieException(movieId, $"Network error while contacting Movie API: {message}", inner);

public class MovieAlreadyExistsException(EntityId<Movie> movieId, int externalId)
    : MovieException(movieId, $"Movie already exists with id '{externalId}'.");

public class MovieApiParsingException(EntityId<Movie> movieId, string message)
    : MovieException(movieId, $"Failed to parse response from Movie API: {message}");

public class UnhandledMovieException(
    EntityId<Movie> movieId,
    Exception? innerException = null)
    : MovieException(movieId, "Unexpected error occurred.", innerException);

