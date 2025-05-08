using System.Net.Sockets;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;
using CommunityToolkit.Aspire.Hosting.Minio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Minio resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class MinioBuilderExtensions
{
    private const string RootUserEnvVarName = "MINIO_ROOT_USER";
    private const string RootPasswordEnvVarName = "MINIO_ROOT_PASSWORD";

    /// <summary>
    /// Adds a Minio container to the application model. The default image is "minio/minio" and the tag is "latest".
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="minioAdminPort">The host port for Minio Admin.</param>
    /// <param name="minioPort">The host port for Minio.</param>
    /// <param name="rootUser">The root user for the Minio server.</param>
    /// <param name="rootPassword">The password for the Minio root user.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{MinioContainerResource}"/>.</returns>
    public static IResourceBuilder<MinioContainerResource> AddMinioContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        string rootUser,
        string rootPassword,
        int minioPort = 9000,
        int minioAdminPort = 9001)
    {
        var minioContainer = new MinioContainerResource(name, rootUser, rootPassword);

        var apiEndpointName = "api";
        
        var builderWithResource = builder
            .AddResource(minioContainer)
            .WithManifestPublishingCallback(context => WriteMinioContainerToManifest(context, minioContainer))
            .WithAnnotation(new EndpointAnnotation(ProtocolType.Tcp, port: minioPort, targetPort: 9000, name: apiEndpointName, uriScheme: "http"))
            .WithAnnotation(new EndpointAnnotation(ProtocolType.Tcp, port: minioAdminPort, targetPort: 9001, name: "http", uriScheme: "http"))
            .WithAnnotation(new ContainerImageAnnotation { Image = "minio/minio", Tag = "latest" })
            .WithEnvironment("MINIO_ADDRESS", ":9000")
            .WithEnvironment("MINIO_CONSOLE_ADDRESS", ":9001")
            .WithEnvironment("MINIO_PROMETHEUS_AUTH_TYPE", "public")
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add(RootUserEnvVarName, minioContainer.RootUser);
                context.EnvironmentVariables.Add(RootPasswordEnvVarName, minioContainer.RootPassword);
            })
            .WithArgs("server", "/data");

        var endpoint = builderWithResource.Resource.GetEndpoint(apiEndpointName);
        var healthCheckKey = $"{name}_check";
        
        builder.Services.AddHealthChecks()
            .Add(new HealthCheckRegistration(
                healthCheckKey,
                sp => new MinioHealthCheck(endpoint.Url),
                failureStatus: default,
                tags: default,
                timeout: default));

        builderWithResource.WithHealthCheck(healthCheckKey);
        
        return builderWithResource;
    }

    private static async Task WriteMinioContainerToManifest(ManifestPublishingContext context, MinioContainerResource resource)
    {
        // Want to see if there is interest 
        await context.WriteContainerAsync(resource);
    }
}