using Cinema.Application.Common.Models.Gemini;
using Refit;

namespace Cinema.Application.Common.Interfaces;

public interface IGeminiApi
{
    [Post("/models/embedding-001:embedContent?key={apiKey}")]
    Task<GeminiResponse> GenerateEmbeddingAsync(string apiKey, [Body] GeminiRequest request);
}