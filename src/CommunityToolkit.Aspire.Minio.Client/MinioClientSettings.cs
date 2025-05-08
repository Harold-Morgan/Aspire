using Microsoft.Extensions.DependencyInjection;

namespace CommunityToolkit.Aspire.Minio.Client;

/// <summary>
/// Minio client configuration
/// </summary>
public class MinioClientSettings
{
    /// <summary>
    /// Endpoint URL
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Access key
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Secret key
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Use ssl connection
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Minio client service lifetime
    /// </summary>
    public ServiceLifetime ServiceLifetime = ServiceLifetime.Singleton;
}
