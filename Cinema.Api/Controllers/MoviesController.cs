using Cinema.Api.Modules.Errors;
using Cinema.Application.Movies.Commands;
using Cinema.Application.Movies.Dtos;
using Cinema.Application.Movies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("movies")]
public class MoviesController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MovieResultSearchDto>> SearchMovies(
        [FromQuery] MovieSearchQuery searchQuery,
        CancellationToken cancellationToken)
    {
        var movies = await sender.Send(searchQuery, cancellationToken);

        return movies.Match<ActionResult<MovieResultSearchDto>>(
            e => Ok(e),
            e => e.ToObjectResult());
    }


    [HttpPost]
    public async Task<ActionResult<MovieResultSearchDto>> CreateMovie(
        [FromBody] int id,
        CancellationToken cancellationToken)
    {
        var command = new CreateMovieCommand
        {
            Id = id,
        };

        var movie = await sender.Send(command, cancellationToken);

        return movie.Match<ActionResult<MovieResultSearchDto>>(
            e => CreatedAtAction(nameof(GetById), new { id = e.Id }, e),
            e => e.ToObjectResult());
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<MovieResultSearchDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}