using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace CommunityToolkit.Aspire.Hosting.Garage.Tests;

public class GaragePublicApiTests
{
    [Fact]
    public void AddGarageContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;

        var action = () => builder.AddGarageContainer("garage");

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void AddGarageContainerShouldThrowWhenNameIsNullOrEmpty()
    {
        IDistributedApplicationBuilder builder = new DistributedApplicationBuilder([]);
        string name = null!;

        var action = () => builder.AddGarageContainer(name);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<GarageContainerResource> builder = null!;

        var action = () => builder.WithDataVolume();

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<GarageContainerResource> builder = null!;

        var action = () => builder.WithDataBindMount("/data");

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        var appBuilder = new DistributedApplicationBuilder([]);
        var resourceBuilder = appBuilder.AddGarageContainer("garage");
        string source = null!;

        var action = () => resourceBuilder.WithDataBindMount(source);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(source), exception.ParamName);
    }

    [Fact]
    public void WithDataBindMountShouldThrowWhenSourceIsEmpty()
    {
        var appBuilder = new DistributedApplicationBuilder([]);
        var resourceBuilder = appBuilder.AddGarageContainer("garage");

        var action = () => resourceBuilder.WithDataBindMount(string.Empty);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void WithMetadataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<GarageContainerResource> builder = null!;

        var action = () => builder.WithMetadataVolume();

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithMetadataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<GarageContainerResource> builder = null!;

        var action = () => builder.WithMetadataBindMount("/meta");

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithMetadataBindMountShouldThrowWhenSourceIsNull()
    {
        var appBuilder = new DistributedApplicationBuilder([]);
        var resourceBuilder = appBuilder.AddGarageContainer("garage");
        string source = null!;

        var action = () => resourceBuilder.WithMetadataBindMount(source);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(source), exception.ParamName);
    }

    [Fact]
    public void WithMetadataBindMountShouldThrowWhenSourceIsEmpty()
    {
        var appBuilder = new DistributedApplicationBuilder([]);
        var resourceBuilder = appBuilder.AddGarageContainer("garage");

        var action = () => resourceBuilder.WithMetadataBindMount(string.Empty);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("source", exception.ParamName);
    }
}
