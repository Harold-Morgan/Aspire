using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityToolkit.Aspire.AwsSdk.S3.Client;

/// <summary>
/// Configuration settings for the AWS S3 (or S3-compatible) client integration.
/// </summary>
public sealed class AwsS3ClientSettings
{
    private const string ConnectionStringEndpoint = "Endpoint";
    private const string AccessKeyKey = "AccessKey";
    private const string SecretKeyKey = "SecretKey";
    private const string RegionKey = "Region";

    /// <summary>
    /// The service URL for the S3 endpoint.
    /// For AWS S3 this is optional (the SDK resolves it from <see cref="Region"/>).
    /// For S3-compatible services such as Garage or MinIO this must be set.
    /// </summary>
    public Uri? ServiceUrl { get; set; }

    /// <inheritdoc cref="AwsS3Credentials" />
    public AwsS3Credentials? Credentials { get; set; }

    /// <summary>
    /// The AWS region (or the region name configured on the S3-compatible service).
    /// Defaults to <c>us-east-1</c>.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Use path-style addressing (<c>http://host/bucket/key</c>) instead of virtual-hosted-style
    /// (<c>http://bucket.host/key</c>). Must be <see langword="true"/> for most S3-compatible
    /// services. Defaults to <see langword="true"/>.
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>
    /// The service lifetime of the registered <see cref="Amazon.S3.IAmazonS3"/> instance.
    /// Defaults to <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;

    internal void ParseConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        // If the value is a bare absolute URI treat it as the service URL.
        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
        {
            ServiceUrl = uri;
            return;
        }

        var connectionBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        if (connectionBuilder.TryGetValue(ConnectionStringEndpoint, out var endpoint)
            && Uri.TryCreate(endpoint.ToString(), UriKind.Absolute, out var serviceUri))
        {
            ServiceUrl = serviceUri;
        }

        if (connectionBuilder.TryGetValue(RegionKey, out var regionObj)
            && regionObj is string region
            && !string.IsNullOrEmpty(region))
        {
            Region = region;
        }

        if (connectionBuilder.TryGetValue(AccessKeyKey, out var accessKeyValue)
            && connectionBuilder.TryGetValue(SecretKeyKey, out var secretKeyValue)
            && accessKeyValue is string accessKey
            && secretKeyValue is string secretKey
            && !string.IsNullOrEmpty(accessKey)
            && !string.IsNullOrEmpty(secretKey))
        {
            Credentials = new AwsS3Credentials
            {
                AccessKey = accessKey,
                SecretKey = secretKey
            };
        }
    }
}

/// <summary>
/// AWS S3 credentials (access key ID and secret access key).
/// </summary>
public sealed class AwsS3Credentials
{
    /// <summary>
    /// The AWS access key ID.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// The AWS secret access key.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
}
