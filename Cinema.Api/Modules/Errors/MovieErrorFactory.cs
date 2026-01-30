using Cinema.Application.Movies.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Modules.Errors;

public static class MovieErrorFactory
{
    public static ObjectResult ToObjectResult(this MovieException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                MovieNotFoundException => StatusCodes.Status404NotFound,
                MovieApiNetworkException => StatusCodes.Status503ServiceUnavailable,
                MovieApiParsingException => StatusCodes.Status502BadGateway,
                UnhandledMovieException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Movie error handler does not implemented.")
            }
        };
    }
}