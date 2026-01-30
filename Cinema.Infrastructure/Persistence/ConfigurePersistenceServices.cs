using Cinema.Application.Common.Interfaces;
using Cinema.Application.Common.Interfaces.Queries;
using Cinema.Application.Common.Settings;
using Cinema.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Cinema.Infrastructure.Persistence;

public static class ConfigurePersistenceServices
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            var settings = provider.GetRequiredService<ApplicationSettings>();
            var connectionString = settings?.ConnectionStrings?.DefaultConnection;
            var dataSource = new NpgsqlDataSourceBuilder(connectionString)
                .EnableDynamicJson()
                .Build();

            options
                .UseNpgsql(dataSource,
                    builder => { builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName); })
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
        });
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<MovieRepository>();
        services.AddScoped<IMovieQueries>(provider => provider.GetRequiredService<MovieRepository>());
    }
}