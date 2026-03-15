using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityToolkit.Aspire.Hosting.Garage.Tests;

public class GarageResourceCreationTests
{
    [Fact]
    public void AddGarageContainerAddsResource()
    {
        var builder = DistributedApplication.CreateBuilder();

        builder.AddGarageContainer("garage");

        using var app = builder.Build();
        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        var resource = Assert.Single(appModel.Resources.OfType<GarageContainerResource>());
        Assert.Equal("garage", resource.Name);
    }

    [Fact]
    public void AddGarageContainerHasS3ApiEndpointOnPort3900()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddGarageContainer("garage");

        using var app = builder.Build();
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<GarageContainerResource>().Single();

        var endpoint = Assert.Single(
            resource.Annotations.OfType<EndpointAnnotation>(),
            e => e.Name == GarageContainerResource.S3ApiEndpointName);
        Assert.Equal(3900, endpoint.TargetPort);
    }

    [Fact]
    public void AddGarageContainerHasAdminEndpointOnPort3903()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddGarageContainer("garage");

        using var app = builder.Build();
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<GarageContainerResource>().Single();

        var endpoint = Assert.Single(
            resource.Annotations.OfType<EndpointAnnotation>(),
            e => e.Name == GarageContainerResource.AdminEndpointName);
        Assert.Equal(3903, endpoint.TargetPort);
    }

    [Fact]
    public void AddGarageContainerRespectsCustomS3Port()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddGarageContainer("garage", s3Port: 19000);

        using var app = builder.Build();
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<GarageContainerResource>().Single();

        var endpoint = Assert.Single(
            resource.Annotations.OfType<EndpointAnnotation>(),
            e => e.Name == GarageContainerResource.S3ApiEndpointName);
        Assert.Equal(19000, endpoint.Port);
    }

    [Fact]
    public async Task AddGarageContainerConnectionStringHasEndpointFormat()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddGarageContainer("garage")
            .WithEndpoint(GarageContainerResource.S3ApiEndpointName,
                e => e.AllocatedEndpoint = new AllocatedEndpoint(e, "localhost", 3900));

        using var app = builder.Build();
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<GarageContainerResource>().Single();

        var connectionString = await resource.GetConnectionStringAsync();
        Assert.Equal("Endpoint=http://localhost:3900", connectionString);
    }

    [Fact]
    public void WithDataVolumeUsesConfigDataDir()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddGarageContainer("garage", configure: cfg => cfg.DataDir = "/custom/data")
            .WithDataVolume();

        using var app = builder.Build();
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<GarageContainerResource>().Single();

        var volume = Assert.Single(resource.Annotations.OfType<ContainerMountAnnotation>(),
            m => m.Target == "/custom/data");
        Assert.Equal(ContainerMountType.Volume, volume.Type);
    }

    [Fact]
    public void WithMetadataVolumeUsesConfigMetadataDir()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddGarageContainer("garage", configure: cfg => cfg.MetadataDir = "/custom/meta")
            .WithMetadataVolume();

        using var app = builder.Build();
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<GarageContainerResource>().Single();

        var volume = Assert.Single(resource.Annotations.OfType<ContainerMountAnnotation>(),
            m => m.Target == "/custom/meta");
        Assert.Equal(ContainerMountType.Volume, volume.Type);
    }
}
