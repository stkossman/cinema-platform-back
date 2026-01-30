namespace Cinema.Application.Common.Settings;

public class ApplicationSettings
{
    public ConnectionStrings? ConnectionStrings { get; set; }
    public ExternalApiSettings? ExternalApiSettings { get; set; }
}

public class ConnectionStrings
{
    public string? DefaultConnection { get; set; }
}

public class ExternalApiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ImageBaseUrl { get; set; } = string.Empty;
}