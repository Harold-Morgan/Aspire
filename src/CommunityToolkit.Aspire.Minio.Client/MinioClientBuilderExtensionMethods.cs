using Microsoft.Extensions.Hosting;
using Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio.DataModel.Notification;

namespace CommunityToolkit.Aspire.Minio.Client;

/// <summary>
/// 
/// </summary>
public static class MinioClientBuilderExtensionMethods
{
    private const string DefaultConfigSectionName = "Aspire:Minio:Client";
    
    /// <summary>
    /// Adds Minio Client to ASPNet host
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configurationSectionName">Name of the configuration settings section</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It is invoked after the settings are read from the configuration.</param>
    public static void AddMinioClient(
        this IHostApplicationBuilder builder,
        string? configurationSectionName,
        Action<MinioClientSettings>? configureSettings = null)
    {
        var settings = GetMinioClientSettings(builder, configurationSectionName, configureSettings);

        builder.AddMinioInternal(settings);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    private static void AddMinioInternal(this IHostApplicationBuilder builder, MinioClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settings);

        ValidateSettings(builder, settings);

        // Add the Minio client to the service collection.
        builder.Services.AddMinio(
            configureClient => configureClient
                    .WithEndpoint(settings.Endpoint)
                    .WithSSL(settings.UseSsl)
                    .WithCredentials(settings.AccessKey, settings.SecretKey),
            settings.ServiceLifetime
            );
    }
    
    private static MinioClientSettings GetMinioClientSettings(IHostApplicationBuilder builder,
        string? configurationSectionName,
        Action<MinioClientSettings>? configureSettings)
    {
        var settings = new MinioClientSettings();

        builder.Configuration.Bind(configurationSectionName ?? DefaultConfigSectionName, settings);
        
        if (configureSettings is not null)
            configureSettings.Invoke(settings);

        return settings;
    }

    private static void ValidateSettings(
        IHostApplicationBuilder builder,
        MinioClientSettings settings)
    {
        if (settings.Endpoint == null)
            throw new ArgumentNullException(nameof(settings.Endpoint));
    }
}