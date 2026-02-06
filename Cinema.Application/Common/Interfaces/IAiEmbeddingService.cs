namespace Cinema.Application.Common.Interfaces;

public interface IAiEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    Task UpdateMovieEmbeddingAsync(Guid movieId, CancellationToken ct);
}