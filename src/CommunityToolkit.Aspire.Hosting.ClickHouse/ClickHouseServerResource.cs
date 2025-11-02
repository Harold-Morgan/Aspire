using Aspire.Hosting.ApplicationModel;

namespace CommunityToolkit.Aspire.Hosting.ClickHouse;

/// <summary>
/// A resource that represents a ClickHouse db server
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="userName">A parameter that contains the ClickHouse username.</param>
/// <param name="password">A parameter that contains the ClickHouse password.</param>
public sealed class ClickHouseServerResource(
    [ResourceName] string name,
    ParameterResource? userName,
    ParameterResource password
    ) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string DefaultUserName = "default";

    /// <summary>
    /// The ClickHouse default user.
    /// </summary>
    public ParameterResource? UserName { get; set; } = userName;
    
    internal ReferenceExpression UserNameReference =>
        UserName is not null ?
            ReferenceExpression.Create($"{UserName}") :
            ReferenceExpression.Create($"{DefaultUserName}");

    /// <summary>
    /// The ClickHouse root password.
    /// </summary>
    public ParameterResource Password { get; private set; } = password;

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    /// Gets the primary endpoint for the ClickHouse server instance.
    /// </summary>
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    
    /// <summary>
    /// Gets the connection string expression for the ClickHouse
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => GetConnectionString();
    
    /// <summary>
    /// Gets the connection string for the ClickHouse server.
    /// </summary>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A connection string for the ClickHouse server in the form "Server=scheme://host:port;User=username;Password=password"</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
        {
            return connectionStringAnnotation.Resource.GetConnectionStringAsync(cancellationToken);
        }

        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }
    
    /// <summary>
    /// Gets the connection string for the ClickHouse server.
    /// </summary>
    private ReferenceExpression GetConnectionString()
    {
        var builder = new ReferenceExpressionBuilder();

        builder.Append(
            $"Host={PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");

        builder.Append($";Protocol=http");
        builder.Append($";Username={UserNameReference}");
        builder.Append($";Password={Password}");

        return builder.Build();
    }

    internal void SetPassword(ParameterResource password)
    {
        Password = password;
    }
}