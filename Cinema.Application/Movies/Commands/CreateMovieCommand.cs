using Cinema.Application.Common.Interfaces;
using Cinema.Application.Common.Interfaces.Services;
using Cinema.Application.Common.Settings;
using Cinema.Application.Models.Movies;
using Cinema.Application.Movies.Dtos;
using Cinema.Application.Movies.Exceptions;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Movies.Commands;

public record CreateMovieCommand : IRequest<Either<MovieException, MovieDto>>
{
    public required int Id { get; init; }
}

public class CreateMovieCommandHandler(
    IApplicationDbContext context,
    IMovieApiService service,
    ApplicationSettings settings)
    : IRequestHandler<CreateMovieCommand, Either<MovieException, MovieDto>>
{
    public async Task<Either<MovieException, MovieDto>> Handle(
        CreateMovieCommand request,
        CancellationToken cancellationToken)
    {
        var externalMovieResult = await service.GetMovieDetailsAsync(request.Id, cancellationToken);

        return await externalMovieResult.BindAsync(details =>
            CreateMovieWithGenresAsync(details, cancellationToken));
    }

    private async Task<Either<MovieException, MovieDto>> CreateMovieWithGenresAsync(
        ExternalMovieDetails details,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await context.Movies.AnyAsync(m => m.ExternalId == details.ExternalId, cancellationToken);
            if (exists)
            {
                return new MovieAlreadyExistsException(EntityId<Movie>.Empty(), details.ExternalId);
            }


            var externalGenres = details.Genres;
            var externalGenreIds = externalGenres.Select(g => g.ExternalId).ToList();

            var existingGenres = await context.Genres
                .Where(g => externalGenreIds.Contains(g.ExternalId))
                .ToListAsync(cancellationToken);


            var genresToLink = new List<Genre>(existingGenres);
            var newGenresToAdd = new List<Genre>();

            foreach (var externalGenre in externalGenres)
            {
                if (existingGenres.All(g => g.ExternalId != externalGenre.ExternalId))
                {
                    var newGenre = Genre.New(
                        EntityId<Genre>.New(),
                        externalGenre.ExternalId,
                        externalGenre.Name,
                        ""
                    );

                    newGenresToAdd.Add(newGenre);
                    genresToLink.Add(newGenre); 
                }
            }

            if (newGenresToAdd.Count != 0)
            {
                await context.Genres.AddRangeAsync(newGenresToAdd, cancellationToken);
            }

            var movieId = EntityId<Movie>.New();
            var movie = Movie.New(
                movieId,
                details.ExternalId,
                details.Title,
                details.DurationMinutes,
                details.Rating, 
                details.PosterUrl,
                null //
            );

            foreach (var genre in genresToLink)
            {
                var movieGenre = MovieGenre.New(movieId, genre.Id);
                movie.MovieGenres?.Add(movieGenre);
            }

            context.Movies.Add(movie);
            await context.SaveChangesAsync(cancellationToken);

            string imageBaseUrl = settings.ExternalApiSettings.ImageBaseUrl;
            return  MovieDto.FromDomainModel(
                movie,
                imageBaseUrl
            );
        }
        catch (Exception ex)
        {
            return new UnhandledMovieException(EntityId<Movie>.Empty(), ex);
        }
    }
}