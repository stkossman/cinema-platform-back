using System.Net;
using System.Net.Http.Json;
using Cinema.Application.Common.Interfaces.Services;
using Cinema.Application.Common.Models;
using Cinema.Application.Models.Movies;
using Cinema.Application.Movies.Exceptions;
using Cinema.Domain.Common;
using Cinema.Domain.Entities;
using Cinema.Infrastructure.External.MovieApi.Dtos;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace Cinema.Infrastructure.External.MovieApi.Services;

public class MovieApiService(
    HttpClient httpClient,
    ILogger<MovieApiService> logger) : IMovieApiService
{
    public async Task<Either<MovieException, PaginatedResult<ExternalMovieModel>>> SearchMoviesAsync(string query,
        int page,
        CancellationToken cancellationToken)
    {
        return await FetchFromApi(query, page, cancellationToken)
            .MapAsync(MapToDomain);
    }

    private async Task<Either<MovieException, TmdbSearchResponse>> FetchFromApi(string query, int page,
        CancellationToken cancellationToken)
    {
        try
        {
            var response =
                await httpClient.GetAsync(
                    $"search/movie?query={Uri.EscapeDataString(query)}&language=uk-UA&page={page}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("TMDB API Error: {Status} {Content}", response.StatusCode, errorContent);
                return new MovieApiNetworkException(EntityId<Movie>.Empty(), $"API returned {response.StatusCode}");
            }

            var data = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>(cancellationToken);

            return data is not null
                ? data
                : new MovieApiParsingException(EntityId<Movie>.Empty(), "Response content is null");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP Request failed");
            return new MovieApiNetworkException(EntityId<Movie>.Empty(), ex.Message, ex);
        }
    }


    private PaginatedResult<ExternalMovieModel> MapToDomain(TmdbSearchResponse response)
    {
        return new PaginatedResult<ExternalMovieModel>
        {
            Page = response.Page,
            TotalPages = response.TotalPage,
            TotalResults = response.TotalResults,
            Results = response.Results.Select(dto => new ExternalMovieModel
            {
                Title = dto.Title,
                OriginalTitle = dto.OriginalTitle,
                ExternalId = dto.Id,
                Overview = dto.Overview,
                PosterPath = dto.PosterPath,
                ReleaseDate = DateTime.TryParse(dto.ReleaseDate, out var date) ? date : null
            }).ToList()
        };
    }

    public async Task<Either<MovieException, ExternalMovieDetails>> GetMovieDetailsAsync(
        int externalId,
        CancellationToken cancellationToken)
    {
        var url = $"movie/{externalId}?language=uk-UA";

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new MovieNotFoundException(EntityId<Movie>.Empty(), externalId);
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("TMDB API Error: {Status} {Content}", response.StatusCode, errorContent);
                return new MovieApiNetworkException(EntityId<Movie>.Empty(), $"API returned {response.StatusCode}");
            }

            var data = await response.Content.ReadFromJsonAsync<TmdbMovieDetailsResponse>(cancellationToken);

            if (data is null)
            {
                return new MovieApiParsingException(EntityId<Movie>.Empty(), "Response content is null");
            }

            return MapToDetailsDomain(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP Request failed for GetMovieDetails");
            return new MovieApiNetworkException(EntityId<Movie>.Empty(), ex.Message, ex);
        }
    }

    private ExternalMovieDetails MapToDetailsDomain(TmdbMovieDetailsResponse dto)
    {

        return new ExternalMovieDetails
        {
            ExternalId = dto.Id,
            Title = dto.Title,
            PosterUrl = dto.PosterPath,
            DurationMinutes = dto.Runtime ?? 0,
            Rating = dto.VoteAverage,
            Genres = dto.Genres.Select(g => new ExternalGenre
            {
                ExternalId = g.Id,
                Name = g.Name
            }).ToList()
        };
    }
}