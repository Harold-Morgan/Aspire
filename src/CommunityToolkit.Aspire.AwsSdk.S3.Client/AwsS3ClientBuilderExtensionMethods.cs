using Amazon.Runtime;
using Amazon.S3;
using CommunityToolkit.Aspire.AwsSdk.S3.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Provides extension methods for registering AWS S3 client services in an <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class AwsS3ClientBuilderExtensionMethods
{
    private const string DefaultConfigSectionName = "Aspire:AwsSdk:S3:Client";

    /// <summary>
    /// Registers an <see cref="IAmazonS3"/> in the DI container for connecting to AWS S3 or an
    /// S3-compatible service such as Garage.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> used to add services.</param>
    /// <param name="connectionName">
    /// The connection name used to resolve a connection string from
    /// <c>ConnectionStrings</c> configuration. When supplied the connection string is parsed into
    /// <see cref="AwsS3ClientSettings"/> before any delegate overrides are applied.
    /// </param>
    /// <param name="configurationSectionName">
    /// The configuration section from which <see cref="AwsS3ClientSettings"/> is bound.
    /// Defaults to <c>Aspire:AwsSdk:S3:Client</c>.
    /// </param>
    /// <param name="configureSettings">
    /// An optional delegate for customising settings after they have been read from configuration.
    /// </param>
    public static void AddAwsS3Client(
        this IHostApplicationBuilder builder,
        string? connectionName = null,
        string? configurationSectionName = DefaultConfigSectionName,
        Action<AwsS3ClientSettings>? configureSettings = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var settings = new AwsS3ClientSettings();

        builder.Configuration.Bind(configurationSectionName ?? DefaultConfigSectionName, settings);

        if (!string.IsNullOrEmpty(connectionName)
            && builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            settings.ParseConnectionString(connectionString);
        }

        configureSettings?.Invoke(settings);

        IAmazonS3 CreateClient()
        {
            if (settings.ServiceUrl is null)
            {
                throw new InvalidOperationException(
                    "The AWS S3 service URL must be provided in configuration, the connection string, or via the settings delegate.");
            }

            var config = new AmazonS3Config
            {
                ServiceURL = settings.ServiceUrl.ToString(),
                ForcePathStyle = settings.ForcePathStyle,
                AuthenticationRegion = settings.Region,
            };

            if (settings.Credentials is not null)
            {
                var credentials = new BasicAWSCredentials(
                    settings.Credentials.AccessKey,
                    settings.Credentials.SecretKey);

                return new AmazonS3Client(credentials, config);
            }

            return new AmazonS3Client(config);
        }

        switch (settings.ServiceLifetime)
        {
            case ServiceLifetime.Singleton:
                builder.Services.TryAddSingleton<IAmazonS3>(_ => CreateClient());
                break;
            case ServiceLifetime.Scoped:
                builder.Services.TryAddScoped<IAmazonS3>(_ => CreateClient());
                break;
            case ServiceLifetime.Transient:
                builder.Services.TryAddTransient<IAmazonS3>(_ => CreateClient());
                break;
        }
    }
}
