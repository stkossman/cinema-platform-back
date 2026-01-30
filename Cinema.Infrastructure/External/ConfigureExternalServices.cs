using Cinema.Application.Common.Interfaces.Services;
using Cinema.Application.Common.Settings;
using Cinema.Infrastructure.External.MovieApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.Infrastructure.External;

public static class ConfigureExternalServices
{
    public static void AddExternalServices(this IServiceCollection services)
    {
        services.AddHttpClient<IMovieApiService, MovieApiService>((provider, client) =>
        {
            var settings = provider.GetRequiredService<ApplicationSettings>();

            client.BaseAddress = new Uri(settings.ExternalApiSettings.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.ExternalApiSettings.ApiKey}");
            client.DefaultRequestHeaders.Add("accept", "application/json");

            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }
}