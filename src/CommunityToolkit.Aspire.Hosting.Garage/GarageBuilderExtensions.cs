using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Garage;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Garage resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class GarageBuilderExtensions
{
    /// <summary>
    /// Adds a Garage container resource to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="adminTokenParameter">An optional admin token parameter for the Garage.</param>
    /// <param name="s3Port">An optional fixed host port for the S3 API. Assigned randomly by Aspire if not set.</param>
    /// <param name="configure">An optional callback to customise the Garage TOML configuration.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{GarageContainerResource}"/>.</returns>
    public static IResourceBuilder<GarageContainerResource> AddGarageContainer(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource>? adminTokenParameter = null,
        int? s3Port = null,
        Action<GarageConfigurationBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        var config = new GarageConfigurationBuilder();
        configure?.Invoke(config);

        var adminToken = adminTokenParameter?.Resource ?? ParameterResourceBuilderExtensions
            .CreateDefaultPasswordParameter(builder, $"{name}-adminToken");

        var resource = new GarageContainerResource(name, adminToken, config);

        return builder.AddResource(resource)
            .WithImage(GarageContainerImageTags.Image, GarageContainerImageTags.Tag)
            .WithImageRegistry(GarageContainerImageTags.Registry)
            .WithHttpEndpoint(
                targetPort: 3900,
                port: s3Port,
                name: GarageContainerResource.S3ApiEndpointName)
            .WithHttpEndpoint(
                targetPort: 3903,
                name: GarageContainerResource.AdminEndpointName)
            .WithContainerFiles("/etc", async (_, ct) =>
            {
                var token = await resource.AdminToken.GetValueAsync(ct).ConfigureAwait(false);
                return [new ContainerFile { Name = "garage.toml", Contents = config.Build(token!) }];
            });
    }

    /// <summary>
    /// Adds a named volume for the Garage data directory.
    /// The mount path is taken from <see cref="GarageConfigurationBuilder.DataDir"/>.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The volume name. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageContainerResource> WithDataVolume(
        this IResourceBuilder<GarageContainerResource> builder,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "data"),
            builder.Resource.Configuration.DataDir);
    }

    /// <summary>
    /// Adds a bind mount for the Garage data directory.
    /// The container path is taken from <see cref="GarageConfigurationBuilder.DataDir"/>.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageContainerResource> WithDataBindMount(
        this IResourceBuilder<GarageContainerResource> builder,
        string source)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(source);

        return builder.WithBindMount(source, builder.Resource.Configuration.DataDir);
    }

    /// <summary>
    /// Adds a bind mount for the Garage metadata directory.
    /// The container path is taken from <see cref="GarageConfigurationBuilder.MetadataDir"/>.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageContainerResource> WithMetadataBindMount(
        this IResourceBuilder<GarageContainerResource> builder,
        string source)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(source);

        return builder.WithBindMount(source, builder.Resource.Configuration.MetadataDir);
    }

    /// <summary>
    /// Adds a named volume for the Garage metadata directory.
    /// The mount path is taken from <see cref="GarageConfigurationBuilder.MetadataDir"/>.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The volume name. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageContainerResource> WithMetadataVolume(
        this IResourceBuilder<GarageContainerResource> builder,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "metadata"),
            builder.Resource.Configuration.MetadataDir);
    }
}
