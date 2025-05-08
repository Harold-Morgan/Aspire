using Aspire.Hosting;
using Aspire.Hosting.Utils;
using Microsoft.Extensions.Hosting;
using Minio;
using Minio.DataModel.Args;
using Xunit.Abstractions;

namespace CommunityToolkit.Aspire.Hosting.Minio.Tests;

public class StorageTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task StorageGetsCreatedAndUsable()
    {
        using var builder = TestDistributedApplicationBuilder.Create(testOutputHelper);
        var name = "minio";       
        var accessKey = "minioadmin";
        var secretKey = "minioadmin";
        var port = 9000;

        var minio = builder.AddMinioContainer(name, accessKey, secretKey, port);

        await using var app = await builder.BuildAsync();
        
        await app.StartAsync();
        
        var rns = app.Services.GetRequiredService<ResourceNotificationService>();

        await rns.WaitForResourceHealthyAsync(minio.Resource.Name);
        
        var hb = Host.CreateApplicationBuilder();
        
        hb.Services.AddMinio(configureClient => configureClient
            .WithEndpoint("localhost", port)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build());
        
        using var host = hb.Build();

        await host.StartAsync();

        var minioClient = host.Services.GetRequiredService<IMinioClient>();

        var bucketName = "somebucket";
        
        var mbArgs = new MakeBucketArgs()
            .WithBucket(bucketName);
        await minioClient.MakeBucketAsync(mbArgs);

        var res = await minioClient.ListBucketsAsync();

        Assert.NotEmpty(res.Buckets);

        var bytearr = "Hey, I'm using minio client! It's awesome!"u8.ToArray();
        var stream = new MemoryStream(bytearr);

        var objectName = "someobj";
        var contentType = "text/plain";
        
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);
        
        await minioClient.PutObjectAsync(putObjectArgs);
        
        var statObject = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        var meta = await minioClient.StatObjectAsync(statObject);
        
        Assert.NotNull(meta);
        Assert.Equal(contentType, meta.ContentType);
    }
}