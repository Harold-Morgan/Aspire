// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting;
using CommunityToolkit.Aspire.Testing;

namespace CommunityToolkit.Aspire.Hosting.Minio.Tests;

public class AppHostTests(AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_Minio_AppHost> fixture) : IClassFixture<AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_Minio_AppHost>>
{
    [Fact]
    public void MinioResourseGetsAdded()
    {
        var builder = DistributedApplication.CreateBuilder();

        builder.AddMinioContainer( "minio", "user", "password");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<MinioContainerResource>());

        Assert.Equal("minio", resource.Name);
    }
    
    [Fact]
    public async Task ResourceStartsAndRespondsOk()
    {
        var resourceName = "minio";
        await fixture.ResourceNotificationService.WaitForResourceHealthyAsync(resourceName).WaitAsync(TimeSpan.FromMinutes(5));
        var httpClient = fixture.CreateHttpClient(resourceName);

        var response = await httpClient.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void MinioResourceHasHealthCheck()
    {
        var builder = DistributedApplication.CreateBuilder();

        builder.AddMinioContainer( "minio", "user", "password");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = appModel.Resources.OfType<MinioContainerResource>().SingleOrDefault();

        Assert.NotNull(resource);

        Assert.Equal("minio", resource.Name);

        var result = resource.TryGetAnnotationsOfType<HealthCheckAnnotation>(out var annotations);

        Assert.True(result);
        Assert.NotNull(annotations);

        Assert.Single(annotations);
    }
}
