using System.Security.Cryptography;
using System.Text;

namespace CommunityToolkit.Aspire.Hosting.Garage;

/// <summary>
/// Builds a Garage TOML configuration file for single-node mode.
/// All properties map directly to the Garage configuration reference:
/// https://garagehq.deuxfleurs.fr/documentation/reference-manual/configuration/
/// </summary>
public sealed class GarageConfigurationBuilder
{
    // ── Storage ──────────────────────────────────────────────────────────────

    /// <summary>Directory in which Garage stores its metadata.</summary>
    public string MetadataDir { get; set; } = "/var/lib/garage/meta";

    /// <summary>Directory in which Garage stores the data blocks.</summary>
    public string DataDir { get; set; } = "/var/lib/garage/data";

    /// <summary>Location for metadata database snapshots. Defaults to &lt;metadata_dir&gt;/snapshots/.</summary>
    public string? MetadataSnapshotsDir { get; set; }

    /// <summary>Database engine: <c>lmdb</c>, <c>sqlite</c>, or <c>fjall</c>.</summary>
    public string DbEngine { get; set; } = "lmdb";

    /// <summary>Maximum metadata database size (LMDB only). E.g. <c>"1GiB"</c>.</summary>
    public string? LmdbMapSize { get; set; }

    /// <summary>Enable synchronous mode (fsync) for the metadata database engine.</summary>
    public bool MetadataFsync { get; set; }

    /// <summary>Enable fsync on data blocks after saving to disk.</summary>
    public bool DataFsync { get; set; }

    /// <summary>Disable the monthly data directory scrubbing check.</summary>
    public bool DisableScrub { get; set; }

    /// <summary>Auto-snapshot interval for the metadata database. E.g. <c>"6h"</c>.</summary>
    public string? MetadataAutoSnapshotInterval { get; set; }

    // ── Data blocks ───────────────────────────────────────────────────────────

    /// <summary>Chunk size for stored objects. E.g. <c>"1M"</c>.</summary>
    public string BlockSize { get; set; } = "1M";

    /// <summary>Maximum RAM buffer for pending data block uploads. E.g. <c>"256MiB"</c>.</summary>
    public string BlockRamBufferMax { get; set; } = "256MiB";

    /// <summary>Maximum number of simultaneous block reads.</summary>
    public int? BlockMaxConcurrentReads { get; set; }

    /// <summary>Maximum parallel block writes per incoming request.</summary>
    public int BlockMaxConcurrentWritesPerRequest { get; set; } = 3;

    /// <summary>Zstd compression level (-99 to 22). Use <c>0</c> for the Zstd default.</summary>
    public int CompressionLevel { get; set; } = 1;

    // ── Consistency / misc ────────────────────────────────────────────────────

    /// <summary>Read/write consistency mode: <c>consistent</c>, <c>degraded</c>, or <c>dangerous</c>.</summary>
    public string ConsistencyMode { get; set; } = "consistent";

    /// <summary>Run the lifecycle worker at midnight in the local timezone instead of UTC.</summary>
    public bool UseLocalTz { get; set; }

    /// <summary>Allow punycode in bucket names.</summary>
    public bool AllowPunycode { get; set; }

    /// <summary>Bypass the world-readable secret file permission check.</summary>
    public bool AllowWorldReadableSecrets { get; set; }

    // ── RPC ───────────────────────────────────────────────────────────────────

    /// <summary>Address and port for inter-cluster RPC communication.</summary>
    public string RpcBindAddr { get; set; } = "[::]:3901";

    /// <summary>Address and port that other cluster nodes use to reach this node.</summary>
    public string RpcPublicAddr { get; set; } = "127.0.0.1:3901";

    /// <summary>Pre-bind outgoing RPC sockets to the listen address.</summary>
    public bool RpcBindOutgoing { get; set; }

    /// <summary>Filter auto-discovered public IPs to a specific subnet CIDR.</summary>
    public string? RpcPublicAddrSubnet { get; set; }

    /// <summary>Bootstrap peer identifiers for cluster formation.</summary>
    public List<string> BootstrapPeers { get; } = [];

    // ── S3 API ────────────────────────────────────────────────────────────────

    /// <summary>IP and port on which to listen for S3 API calls.</summary>
    public string S3ApiBindAddr { get; set; } = "[::]:3900";

    /// <summary>S3 region name reported to clients.</summary>
    public string S3Region { get; set; } = "garage";

    /// <summary>Optional suffix for virtual-hosted-style bucket access. E.g. <c>".s3.garage.localhost"</c>.</summary>
    public string? S3RootDomain { get; set; }

    // ── S3 Web ────────────────────────────────────────────────────────────────

    /// <summary>Enable the S3 static-website endpoint (<c>[s3_web]</c> section).</summary>
    public bool EnableS3Web { get; set; }

    /// <summary>IP and port for the S3 static-website HTTP endpoint.</summary>
    public string S3WebBindAddr { get; set; } = "[::]:3902";

    /// <summary>Root domain suffix for static-website vhost routing.</summary>
    public string? S3WebRootDomain { get; set; }

    /// <summary>Include the HTTP Host header in Prometheus metrics labels.</summary>
    public bool S3WebAddHostToMetrics { get; set; }

    // ── Admin ─────────────────────────────────────────────────────────────────

    /// <summary>IP and port for the Garage admin HTTP API.</summary>
    public string AdminApiBindAddr { get; set; } = "[::]:3903";

    /// <summary>Optional token required to access Prometheus metrics endpoint.</summary>
    public string? MetricsToken { get; set; }

    /// <summary>Require a token for the metrics endpoint (Garage v2.0+).</summary>
    public bool MetricsRequireToken { get; set; }

    /// <summary>URL of an OpenTelemetry collector for trace export.</summary>
    public string? TraceSink { get; set; }

    // ── Discovery ─────────────────────────────────────────────────────────────

    /// <summary>Consul service-discovery configuration. Set to non-null to enable.</summary>
    public ConsulDiscoveryOptions? ConsulDiscovery { get; set; }

    /// <summary>Kubernetes service-discovery configuration. Set to non-null to enable.</summary>
    public KubernetesDiscoveryOptions? KubernetesDiscovery { get; set; }

    // ── Internal ──────────────────────────────────────────────────────────────

    private string? _rpcSecret;

    /// <summary>
    /// Auto-generated 32-byte hex RPC secret. Stable for the lifetime of this instance.
    /// Required by Garage even in single-node mode.
    /// </summary>
    internal string RpcSecret
    {
        get
        {
            if (_rpcSecret is null)
            {
                var bytes = new byte[32];
                RandomNumberGenerator.Fill(bytes);
                _rpcSecret = Convert.ToHexString(bytes).ToLowerInvariant();
            }

            return _rpcSecret;
        }
    }

    /// <summary>
    /// Generates the complete TOML configuration string. The admin token is injected
    /// from the resolved <c>ParameterResource</c> at call time.
    /// </summary>
    /// <param name="adminToken">The plaintext admin API token value.</param>
    internal string Build(string adminToken)
    {
        var sb = new StringBuilder();

        // Top-level
        sb.AppendLine($"metadata_dir = \"{MetadataDir}\"");
        sb.AppendLine($"data_dir = \"{DataDir}\"");
        sb.AppendLine("replication_factor = 1");
        sb.AppendLine($"consistency_mode = \"{ConsistencyMode}\"");

        if (MetadataSnapshotsDir is not null)
        {
            sb.AppendLine($"metadata_snapshots_dir = \"{MetadataSnapshotsDir}\"");
        }

        sb.AppendLine($"db_engine = \"{DbEngine}\"");

        if (LmdbMapSize is not null)
        {
            sb.AppendLine($"lmdb_map_size = \"{LmdbMapSize}\"");
        }

        sb.AppendLine($"metadata_fsync = {BoolToToml(MetadataFsync)}");
        sb.AppendLine($"data_fsync = {BoolToToml(DataFsync)}");
        sb.AppendLine($"disable_scrub = {BoolToToml(DisableScrub)}");
        sb.AppendLine($"use_local_tz = {BoolToToml(UseLocalTz)}");
        sb.AppendLine($"allow_punycode = {BoolToToml(AllowPunycode)}");
        sb.AppendLine($"allow_world_readable_secrets = {BoolToToml(AllowWorldReadableSecrets)}");

        if (MetadataAutoSnapshotInterval is not null)
        {
            sb.AppendLine($"metadata_auto_snapshot_interval = \"{MetadataAutoSnapshotInterval}\"");
        }

        sb.AppendLine($"block_size = \"{BlockSize}\"");
        sb.AppendLine($"block_ram_buffer_max = \"{BlockRamBufferMax}\"");

        if (BlockMaxConcurrentReads.HasValue)
        {
            sb.AppendLine($"block_max_concurrent_reads = {BlockMaxConcurrentReads.Value}");
        }

        sb.AppendLine($"block_max_concurrent_writes_per_request = {BlockMaxConcurrentWritesPerRequest}");
        sb.AppendLine($"compression_level = {CompressionLevel}");
        sb.AppendLine($"rpc_secret = \"{RpcSecret}\"");
        sb.AppendLine($"rpc_bind_addr = \"{RpcBindAddr}\"");
        sb.AppendLine($"rpc_public_addr = \"{RpcPublicAddr}\"");
        sb.AppendLine($"rpc_bind_outgoing = {BoolToToml(RpcBindOutgoing)}");

        if (RpcPublicAddrSubnet is not null)
        {
            sb.AppendLine($"rpc_public_addr_subnet = \"{RpcPublicAddrSubnet}\"");
        }

        if (BootstrapPeers.Count > 0)
        {
            sb.AppendLine("bootstrap_peers = [");
            foreach (var peer in BootstrapPeers)
            {
                sb.AppendLine($"  \"{peer}\",");
            }
            sb.AppendLine("]");
        }

        // [s3_api]
        sb.AppendLine();
        sb.AppendLine("[s3_api]");
        sb.AppendLine($"api_bind_addr = \"{S3ApiBindAddr}\"");
        sb.AppendLine($"s3_region = \"{S3Region}\"");

        if (S3RootDomain is not null)
        {
            sb.AppendLine($"root_domain = \"{S3RootDomain}\"");
        }

        // [s3_web] — opt-in only
        if (EnableS3Web)
        {
            sb.AppendLine();
            sb.AppendLine("[s3_web]");
            sb.AppendLine($"bind_addr = \"{S3WebBindAddr}\"");
            sb.AppendLine($"root_domain = \"{S3WebRootDomain ?? ".web.garage.localhost"}\"");
            sb.AppendLine($"add_host_to_metrics = {BoolToToml(S3WebAddHostToMetrics)}");
        }

        // [admin]
        sb.AppendLine();
        sb.AppendLine("[admin]");
        sb.AppendLine($"api_bind_addr = \"{AdminApiBindAddr}\"");
        sb.AppendLine($"admin_token = \"{adminToken}\"");

        if (MetricsToken is not null)
        {
            sb.AppendLine($"metrics_token = \"{MetricsToken}\"");
        }

        if (MetricsRequireToken)
        {
            sb.AppendLine("metrics_require_token = true");
        }

        if (TraceSink is not null)
        {
            sb.AppendLine($"trace_sink = \"{TraceSink}\"");
        }

        // [consul_discovery]
        if (ConsulDiscovery is not null)
        {
            sb.AppendLine();
            sb.AppendLine("[consul_discovery]");

            if (ConsulDiscovery.Api is not null)
            {
                sb.AppendLine($"api = \"{ConsulDiscovery.Api}\"");
            }

            if (ConsulDiscovery.ConsulHttpAddr is not null)
            {
                sb.AppendLine($"consul_http_addr = \"{ConsulDiscovery.ConsulHttpAddr}\"");
            }

            if (ConsulDiscovery.ServiceName is not null)
            {
                sb.AppendLine($"service_name = \"{ConsulDiscovery.ServiceName}\"");
            }

            sb.AppendLine($"tls_skip_verify = {BoolToToml(ConsulDiscovery.TlsSkipVerify)}");

            if (ConsulDiscovery.CaCert is not null)
            {
                sb.AppendLine($"ca_cert = \"{ConsulDiscovery.CaCert}\"");
            }

            if (ConsulDiscovery.ClientCert is not null)
            {
                sb.AppendLine($"client_cert = \"{ConsulDiscovery.ClientCert}\"");
            }

            if (ConsulDiscovery.ClientKey is not null)
            {
                sb.AppendLine($"client_key = \"{ConsulDiscovery.ClientKey}\"");
            }

            if (ConsulDiscovery.Token is not null)
            {
                sb.AppendLine($"token = \"{ConsulDiscovery.Token}\"");
            }

            if (ConsulDiscovery.Tags.Count > 0)
            {
                sb.AppendLine("tags = [");
                foreach (var tag in ConsulDiscovery.Tags)
                {
                    sb.AppendLine($"  \"{tag}\",");
                }
                sb.AppendLine("]");
            }

            if (ConsulDiscovery.Meta.Count > 0)
            {
                sb.AppendLine("[consul_discovery.meta]");
                foreach (var (k, v) in ConsulDiscovery.Meta)
                {
                    sb.AppendLine($"{k} = \"{v}\"");
                }
            }

            if (ConsulDiscovery.Datacenters.Count > 0)
            {
                sb.AppendLine("datacenters = [");
                foreach (var dc in ConsulDiscovery.Datacenters)
                {
                    sb.AppendLine($"  \"{dc}\",");
                }
                sb.AppendLine("]");
            }
        }

        // [kubernetes_discovery]
        if (KubernetesDiscovery is not null)
        {
            sb.AppendLine();
            sb.AppendLine("[kubernetes_discovery]");

            if (KubernetesDiscovery.Namespace is not null)
            {
                sb.AppendLine($"namespace = \"{KubernetesDiscovery.Namespace}\"");
            }

            if (KubernetesDiscovery.ServiceName is not null)
            {
                sb.AppendLine($"service_name = \"{KubernetesDiscovery.ServiceName}\"");
            }

            sb.AppendLine($"skip_crd = {BoolToToml(KubernetesDiscovery.SkipCrd)}");
        }

        return sb.ToString();
    }

    private static string BoolToToml(bool value) => value ? "true" : "false";
}

/// <summary>
/// Configuration for Consul-based cluster discovery.
/// </summary>
public sealed class ConsulDiscoveryOptions
{
    /// <summary>Service registration API: <c>catalog</c> (default) or <c>agent</c>.</summary>
    public string? Api { get; set; }

    /// <summary>Full HTTP(S) address of the Consul server.</summary>
    public string? ConsulHttpAddr { get; set; }

    /// <summary>Service name used when registering the RPC port.</summary>
    public string? ServiceName { get; set; }

    /// <summary>Skip TLS hostname verification.</summary>
    public bool TlsSkipVerify { get; set; }

    /// <summary>Path to TLS CA certificate.</summary>
    public string? CaCert { get; set; }

    /// <summary>Path to TLS client certificate (catalog API only).</summary>
    public string? ClientCert { get; set; }

    /// <summary>Path to TLS client key (catalog API only).</summary>
    public string? ClientKey { get; set; }

    /// <summary>Consul API token (agent API only).</summary>
    public string? Token { get; set; }

    /// <summary>Additional tags added during service registration.</summary>
    public List<string> Tags { get; } = [];

    /// <summary>Service metadata key-value pairs.</summary>
    public Dictionary<string, string> Meta { get; } = [];

    /// <summary>Datacenters for WAN federation discovery.</summary>
    public List<string> Datacenters { get; } = [];
}

/// <summary>
/// Configuration for Kubernetes-based cluster discovery.
/// </summary>
public sealed class KubernetesDiscoveryOptions
{
    /// <summary>Kubernetes namespace for custom resources.</summary>
    public string? Namespace { get; set; }

    /// <summary>Label added to advertised resources for filtering.</summary>
    public string? ServiceName { get; set; }

    /// <summary>Disable automatic CRD creation.</summary>
    public bool SkipCrd { get; set; }
}
