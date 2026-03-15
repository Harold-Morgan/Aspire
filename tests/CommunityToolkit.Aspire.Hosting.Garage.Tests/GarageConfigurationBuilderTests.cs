namespace CommunityToolkit.Aspire.Hosting.Garage.Tests;

public class GarageConfigurationBuilderTests
{
    [Fact]
    public void BuildContainsReplicationFactor1()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.Contains("replication_factor = 1", toml);
    }

    [Fact]
    public void BuildContainsDefaultMetadataAndDataDirs()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.Contains("metadata_dir = \"/var/lib/garage/meta\"", toml);
        Assert.Contains("data_dir = \"/var/lib/garage/data\"", toml);
    }

    [Fact]
    public void BuildRespectsCustomDirs()
    {
        var builder = new GarageConfigurationBuilder
        {
            MetadataDir = "/custom/meta",
            DataDir     = "/custom/data"
        };
        var toml = builder.Build("token");
        Assert.Contains("metadata_dir = \"/custom/meta\"", toml);
        Assert.Contains("data_dir = \"/custom/data\"", toml);
    }

    [Fact]
    public void BuildInjectsAdminToken()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("my-secret-token");
        Assert.Contains("admin_token = \"my-secret-token\"", toml);
    }

    [Fact]
    public void BuildContainsAdminSection()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.Contains("[admin]", toml);
        Assert.Contains("api_bind_addr = \"[::]:3903\"", toml);
    }

    [Fact]
    public void BuildContainsS3ApiSection()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.Contains("[s3_api]", toml);
        Assert.Contains("s3_region = \"garage\"", toml);
        Assert.Contains("api_bind_addr = \"[::]:3900\"", toml);
    }

    [Fact]
    public void BuildOmitsS3WebSectionByDefault()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.DoesNotContain("[s3_web]", toml);
    }

    [Fact]
    public void BuildIncludesS3WebSectionWhenEnabled()
    {
        var builder = new GarageConfigurationBuilder { EnableS3Web = true };
        var toml = builder.Build("token");
        Assert.Contains("[s3_web]", toml);
        Assert.Contains("bind_addr = \"[::]:3902\"", toml);
    }

    [Fact]
    public void BuildOmitsConsulSectionByDefault()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.DoesNotContain("[consul_discovery]", toml);
    }

    [Fact]
    public void BuildIncludesConsulSectionWhenSet()
    {
        var builder = new GarageConfigurationBuilder
        {
            ConsulDiscovery = new ConsulDiscoveryOptions
            {
                ConsulHttpAddr = "http://consul:8500",
                ServiceName    = "garage"
            }
        };
        var toml = builder.Build("token");
        Assert.Contains("[consul_discovery]", toml);
        Assert.Contains("consul_http_addr = \"http://consul:8500\"", toml);
        Assert.Contains("service_name = \"garage\"", toml);
    }

    [Fact]
    public void BuildOmitsKubernetesSectionByDefault()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.DoesNotContain("[kubernetes_discovery]", toml);
    }

    [Fact]
    public void RpcSecretIsStableAcrossMultipleBuildCalls()
    {
        var builder = new GarageConfigurationBuilder();
        var toml1 = builder.Build("t1");
        var toml2 = builder.Build("t2");

        var secret1 = toml1.Split('\n').Single(l => l.StartsWith("rpc_secret"));
        var secret2 = toml2.Split('\n').Single(l => l.StartsWith("rpc_secret"));
        Assert.Equal(secret1, secret2);
    }

    [Fact]
    public void RpcSecretIsA64CharHexString()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        var line = toml.Split('\n').Single(l => l.StartsWith("rpc_secret"));
        var secret = line.Split('"')[1];
        Assert.Equal(64, secret.Length);
        Assert.Matches("^[0-9a-f]{64}$", secret);
    }

    [Fact]
    public void OptionalTopLevelFieldsOmittedByDefault()
    {
        var builder = new GarageConfigurationBuilder();
        var toml = builder.Build("token");
        Assert.DoesNotContain("metadata_snapshots_dir", toml);
        Assert.DoesNotContain("lmdb_map_size", toml);
        Assert.DoesNotContain("block_max_concurrent_reads", toml);
        Assert.DoesNotContain("rpc_public_addr_subnet", toml);
        Assert.DoesNotContain("metrics_token", toml);
        Assert.DoesNotContain("trace_sink", toml);
    }
}
