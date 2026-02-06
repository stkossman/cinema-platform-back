using Cinema.Application.Common.Interfaces;
using Cinema.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cinema.Application.Movies.EventHandlers;

public class GenerateMovieEmbeddingHandler(
    IAiEmbeddingService aiService,
    IApplicationDbContext context,
    ILogger<GenerateMovieEmbeddingHandler> logger)
    : INotificationHandler<MovieCreatedEvent>
{
    public async Task Handle(MovieCreatedEvent notification, CancellationToken cancellationToken)
    {
        var movie = notification.Movie;

        var textToEmbed = $"Movie: {movie.Title}. Description: {movie.Description ?? "No description"}";

        try
        {
            var embedding = await aiService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);
            movie.SetEmbedding(embedding);
            await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Embedding generated for movie {Title}", movie.Title);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate embedding for movie {Title}", movie.Title);
        }
    }
}