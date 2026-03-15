using Aspire.Components.Common.Tests;
using Aspire.Hosting;
using Aspire.Hosting.Utils;
using CommunityToolkit.Aspire.Testing;
using Microsoft.Extensions.Hosting;
using Minio;
using Minio.DataModel.Args;

namespace CommunityToolkit.Aspire.Hosting.Garage.Tests;

[RequiresDocker]
public class GarageFunctionalTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task StorageGetsCreatedAndUsable()
    {
        using var distributedApplicationBuilder = TestDistributedApplicationBuilder.Create(testOutputHelper);
        var rootUser = "garageadmin";

        var passwordParameter = ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(distributedApplicationBuilder,
            $"rootPassword");
        distributedApplicationBuilder.Configuration["Parameters:rootPassword"] = await passwordParameter.GetValueAsync(default);
        var rootPasswordParameter = distributedApplicationBuilder.AddParameter(passwordParameter.Name);

        var garage = distributedApplicationBuilder
            .AddGarageContainer("garage", rootPasswordParameter);

        var garageEndpoint = garage.GetEndpoint(GarageContainerResource.S3ApiEndpointName);

        await using var app = await distributedApplicationBuilder.BuildAsync();
        
        await app.StartAsync();
        
        var rns = app.Services.GetRequiredService<ResourceNotificationService>();

        await rns.WaitForResourceHealthyAsync(garage.Resource.Name);
        
        var webApplicationBuilder = Host.CreateApplicationBuilder();
        
        // Adds minio client
        webApplicationBuilder.Services.AddMinio(async configureClient => configureClient
            .WithEndpoint("localhost", garageEndpoint.Port)
            .WithCredentials(rootUser, await passwordParameter.GetValueAsync(default))
            .WithSSL(false)
            .Build());
        
        using var host = webApplicationBuilder.Build();

        await host.StartAsync();

        var minioClient = host.Services.GetRequiredService<IMinioClient>();

        await TestApi(minioClient);
    }

    private static async Task TestApi(IMinioClient minioClient, bool isDataPreGenerated = true)
    {
        const string bucketName = "somebucket";
        const string objectName = "someobj";
        const string contentType = "text/plain";
        
        if (isDataPreGenerated)
        {
            var mbArgs = new MakeBucketArgs()
                .WithBucket(bucketName);
            await minioClient.MakeBucketAsync(mbArgs);

            var res = await minioClient.ListBucketsAsync();

            Assert.NotEmpty(res.Buckets);

            var bytearr = "Hey, I'm using minio client! It's awesome!"u8.ToArray();
            var stream = new MemoryStream(bytearr);
        
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);
        
            await minioClient.PutObjectAsync(putObjectArgs);
        }
        
        var statObject = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        var meta = await minioClient.StatObjectAsync(statObject);
        
        Assert.NotNull(meta);
        Assert.Equal(contentType, meta.ContentType);
    }
}