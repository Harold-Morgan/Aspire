using CommunityToolkit.Aspire.Hosting.Garage;

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a Garage S3-compatible object storage container.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="adminToken">A parameter that contains the Garage admin API token.</param>
/// <param name="configuration">The Garage configuration builder used to generate the TOML config file.</param>
public sealed class GarageContainerResource(
    string name,
    ParameterResource adminToken,
    GarageConfigurationBuilder configuration)
    : ContainerResource(name), IResourceWithConnectionString
{
    internal const string S3ApiEndpointName = "s3api";
    internal const string AdminEndpointName = "admin";

    /// <summary>
    /// Gets the Garage admin API token parameter.
    /// </summary>
    public ParameterResource AdminToken { get; } = adminToken;

    /// <summary>
    /// Gets the configuration builder used to generate the Garage TOML config.
    /// </summary>
    internal GarageConfigurationBuilder Configuration { get; } = configuration;

    /// <summary>
    /// Gets the S3 API endpoint reference.
    /// </summary>
    public EndpointReference S3ApiEndpoint =>
        field ??= new(this, S3ApiEndpointName);

    /// <summary>
    /// Gets the connection string expression for the Garage S3 endpoint.
    /// </summary>
    /// <remarks>
    /// Format: <c>Endpoint=http://{host}:{port}</c>. AccessKey and SecretKey must be
    /// provisioned separately via the Garage admin API after the container starts.
    /// </remarks>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"Endpoint=http://{S3ApiEndpoint.Property(EndpointProperty.Host)}:{S3ApiEndpoint.Property(EndpointProperty.Port)}");

    /// <inheritdoc/>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var redirect))
        {
            return redirect.Resource.GetConnectionStringAsync(cancellationToken);
        }
        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<string, ReferenceExpression>> IResourceWithConnectionString.GetConnectionProperties()
    {
        yield return new("Endpoint", ReferenceExpression.Create(
            $"http://{S3ApiEndpoint.Property(EndpointProperty.Host)}:{S3ApiEndpoint.Property(EndpointProperty.Port)}"));
    }
}
