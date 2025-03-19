//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Aspire.Hosting
{
    public static partial class DaprMetadataResourceBuilderExtensions
    {
        public static ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> WithMetadata(this ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> builder, string name, ApplicationModel.ParameterResource parameterResource) { throw null; }

        public static ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> WithMetadata(this ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> builder, string name, string value) { throw null; }
    }

    public static partial class IDistributedApplicationBuilderExtensions
    {
        public static IDistributedApplicationBuilder AddDapr(this IDistributedApplicationBuilder builder, System.Action<CommunityToolkit.Aspire.Hosting.Dapr.DaprOptions>? configure = null) { throw null; }

        public static ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> AddDaprComponent(this IDistributedApplicationBuilder builder, string name, string type, CommunityToolkit.Aspire.Hosting.Dapr.DaprComponentOptions? options = null) { throw null; }

        public static ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> AddDaprPubSub(this IDistributedApplicationBuilder builder, string name, CommunityToolkit.Aspire.Hosting.Dapr.DaprComponentOptions? options = null) { throw null; }

        public static ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> AddDaprStateStore(this IDistributedApplicationBuilder builder, string name, CommunityToolkit.Aspire.Hosting.Dapr.DaprComponentOptions? options = null) { throw null; }
    }

    public static partial class IDistributedApplicationResourceBuilderExtensions
    {
        public static ApplicationModel.IResourceBuilder<T> WithDaprSidecar<T>(this ApplicationModel.IResourceBuilder<T> builder, CommunityToolkit.Aspire.Hosting.Dapr.DaprSidecarOptions? options = null)
            where T : ApplicationModel.IResource { throw null; }

        public static ApplicationModel.IResourceBuilder<T> WithDaprSidecar<T>(this ApplicationModel.IResourceBuilder<T> builder, System.Action<ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprSidecarResource>> configureSidecar)
            where T : ApplicationModel.IResource { throw null; }

        public static ApplicationModel.IResourceBuilder<T> WithDaprSidecar<T>(this ApplicationModel.IResourceBuilder<T> builder, string appId)
            where T : ApplicationModel.IResource { throw null; }

        public static ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprSidecarResource> WithOptions(this ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprSidecarResource> builder, CommunityToolkit.Aspire.Hosting.Dapr.DaprSidecarOptions options) { throw null; }

        public static ApplicationModel.IResourceBuilder<TDestination> WithReference<TDestination>(this ApplicationModel.IResourceBuilder<TDestination> builder, ApplicationModel.IResourceBuilder<CommunityToolkit.Aspire.Hosting.Dapr.IDaprComponentResource> component)
            where TDestination : ApplicationModel.IResource { throw null; }
    }
}

namespace CommunityToolkit.Aspire.Hosting.Dapr
{
    public sealed partial record DaprComponentOptions()
    {
        public string? LocalPath { get { throw null; } init { } }
    }

    public sealed partial record DaprComponentReferenceAnnotation(IDaprComponentResource Component) : global::Aspire.Hosting.ApplicationModel.IResourceAnnotation
    {
    }

    public sealed partial class DaprComponentResource : global::Aspire.Hosting.ApplicationModel.Resource, IDaprComponentResource, global::Aspire.Hosting.ApplicationModel.IResource, global::Aspire.Hosting.ApplicationModel.IResourceWithWaitSupport
    {
        public DaprComponentResource(string name, string type) : base(default!) { }

        public DaprComponentOptions? Options { get { throw null; } init { } }

        public string Type { get { throw null; } }
    }

    public abstract partial class DaprComponentSpecMetadata
    {
        [YamlDotNet.Serialization.YamlMember(Order = 1)]
        public required string Name { get { throw null; } init { } }
    }

    public sealed partial class DaprComponentSpecMetadataSecret : DaprComponentSpecMetadata
    {
        [YamlDotNet.Serialization.YamlMember(Order = 2)]
        public required DaprSecretKeyRef SecretKeyRef { get { throw null; } set { } }
    }

    public sealed partial class DaprComponentSpecMetadataValue : DaprComponentSpecMetadata
    {
        [YamlDotNet.Serialization.YamlMember(Order = 2)]
        public required string Value { get { throw null; } set { } }
    }

    public sealed partial record DaprOptions()
    {
        public string? DaprPath { get { throw null; } set { } }

        public bool? EnableTelemetry { get { throw null; } set { } }
    }

    public sealed partial class DaprSecretKeyRef
    {
        public required string Key { get { throw null; } init { } }

        public required string Name { get { throw null; } init { } }
    }

    public sealed partial record DaprSidecarAnnotation(IDaprSidecarResource Sidecar) : global::Aspire.Hosting.ApplicationModel.IResourceAnnotation
    {
    }

    public sealed partial record DaprSidecarOptions()
    {
        public string? AppChannelAddress { get { throw null; } init { } }

        public string? AppEndpoint { get { throw null; } init { } }

        public string? AppHealthCheckPath { get { throw null; } init { } }

        public int? AppHealthProbeInterval { get { throw null; } init { } }

        public int? AppHealthProbeTimeout { get { throw null; } init { } }

        public int? AppHealthThreshold { get { throw null; } init { } }

        public string? AppId { get { throw null; } init { } }

        public int? AppMaxConcurrency { get { throw null; } init { } }

        public int? AppPort { get { throw null; } init { } }

        public string? AppProtocol { get { throw null; } init { } }

        public System.Collections.Immutable.IImmutableList<string> Command { get { throw null; } init { } }

        public string? Config { get { throw null; } init { } }

        public int? DaprGrpcPort { get { throw null; } init { } }

        [System.Obsolete("Use DaprMaxBodySize", false)]
        public int? DaprHttpMaxRequestSize { get { throw null; } init { } }

        public int? DaprHttpPort { get { throw null; } init { } }

        [System.Obsolete("Use DaprMaxBodySize", false)]
        public int? DaprHttpReadBufferSize { get { throw null; } init { } }

        public int? DaprInternalGrpcPort { get { throw null; } init { } }

        public string? DaprListenAddresses { get { throw null; } init { } }

        public string? DaprMaxBodySize { get { throw null; } init { } }

        public string? DaprReadBufferSize { get { throw null; } init { } }

        public bool? EnableApiLogging { get { throw null; } init { } }

        public bool? EnableAppHealthCheck { get { throw null; } init { } }

        public bool? EnableProfiling { get { throw null; } init { } }

        public string? LogLevel { get { throw null; } init { } }

        public int? MetricsPort { get { throw null; } init { } }

        public string? PlacementHostAddress { get { throw null; } init { } }

        public int? ProfilePort { get { throw null; } init { } }

        public System.Collections.Immutable.IImmutableSet<string> ResourcesPaths { get { throw null; } init { } }

        public string? RunFile { get { throw null; } init { } }

        public string? RuntimePath { get { throw null; } init { } }

        public string? SchedulerHostAddress { get { throw null; } init { } }

        public string? UnixDomainSocket { get { throw null; } init { } }
    }

    public sealed partial record DaprSidecarOptionsAnnotation(DaprSidecarOptions Options) : global::Aspire.Hosting.ApplicationModel.IResourceAnnotation
    {
    }

    public partial interface IDaprComponentResource : global::Aspire.Hosting.ApplicationModel.IResource, global::Aspire.Hosting.ApplicationModel.IResourceWithWaitSupport
    {
        DaprComponentOptions? Options { get; }

        string Type { get; }
    }

    public partial interface IDaprSidecarResource : global::Aspire.Hosting.ApplicationModel.IResource
    {
    }
}