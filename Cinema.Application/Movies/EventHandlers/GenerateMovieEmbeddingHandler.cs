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
        var result = await aiService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError("Failed to generate embedding for movie {Title}. Error: {ErrorCode} - {ErrorMessage}", 
                movie.Title, result.Error.Code, result.Error.Description);
            return;
        }

        try 
        {
            movie.SetEmbedding(result.Value);
            
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Embedding generated for movie {Title}", movie.Title);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save embedding for movie {Title}", movie.Title);
        }
    }
}