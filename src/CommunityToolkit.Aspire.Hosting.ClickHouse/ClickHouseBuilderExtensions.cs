using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace CommunityToolkit.Aspire.Hosting.ClickHouse;

/// <summary>
/// Provides extension methods for adding ClickHouse resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class ClickHouseBuilderExtensions
{
    private const int ClickHouseDefaultPort = 8123;
    private const string UserEnvVarName = "CLICKHOUSE_USER";
    private const string PasswordEnvVarName = "CLICKHOUSE_PASS";

    /// <summary>
    /// Adds a ClickHouse resource to the application model. A container is used for local development.
    /// The default image is <inheritdoc cref="ClickHouseContainerImageTags.Image"/> and the tag is <inheritdoc cref="ClickHouseContainerImageTags.Tag"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the administrator username for the ClickHouse resource.</param>
    /// <param name="password">The parameter used to provide the administrator password for the ClickHouse resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="port">The host port for the ClickHouse instance.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    /// <remarks>
    /// <example>
    /// Add a ClickHouse container to the application model and reference it in a .NET project.
    /// <code lang="csharp">
    /// var builder = DistributedApplication.CreateBuilder(args);
    ///
    /// var db = builder.AddClickHouseServer("clickhouse")
    ///   .AddDatabase("db");
    /// var api = builder.AddProject&lt;Projects.Api&gt;("api")
    ///   .WithReference(db);
    ///  
    /// builder.Build().Run(); 
    /// </code>
    /// </example>
    /// </remarks>
    public static IResourceBuilder<ClickHouseServerResource> AddClickHouseServer(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null,
        int? port = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);
        
        // The password must be at least 8 characters long and contain characters from three of the following four sets: Uppercase letters, Lowercase letters, Base 10 digits, and Symbols
        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password", minLower: 1, minUpper: 1, minNumeric: 1);
        
        var clickHouseServer = new ClickHouseServerResource(name, userName?.Resource, passwordParameter);

        return builder.AddResource(clickHouseServer)
                      .WithEndpoint(port: port, targetPort: ClickHouseDefaultPort, name: ClickHouseServerResource.PrimaryEndpointName)
                      .WithImage(ClickHouseContainerImageTags.Image, ClickHouseContainerImageTags.Tag)
                      .WithImageRegistry(ClickHouseContainerImageTags.Registry)
                      .WithEnvironment(context =>
                      {
                          context.EnvironmentVariables[UserEnvVarName] = clickHouseServer.UserNameReference;
                          context.EnvironmentVariables[PasswordEnvVarName] = clickHouseServer.Password;
                      })
                      .OnResourceReady(async (_, @event, ct) =>
                      {

                          var connectionString = await clickHouseServer.GetConnectionStringAsync(ct).ConfigureAwait(false);
                          if (connectionString is null)
                          {
                              throw new DistributedApplicationException($"ResourceReadyEvent was published for the '{clickHouseServer.Name}' resource but the connection string was null.");
                          }
                      });
    }
}