namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a MiniO storage
/// </summary>
/// <param name="name">The name of the resource</param>
/// <param name="rootUser">MiniO root user name</param>
/// <param name="rootPassword">MiniO container root user password</param>
public sealed class MinioContainerResource(string name, string rootUser, string rootPassword) : ContainerResource(name)
{
    /// <summary>
    /// The Minio root user.
    /// </summary>
    public string RootUser { get; } = rootUser;

    /// <summary>
    /// The Minio root password.
    /// </summary>
    public string RootPassword { get; } = rootPassword;
}