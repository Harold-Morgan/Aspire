// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Aspire.Hosting;
using Aspire.Hosting.Utils;
using CommunityToolkit.Aspire.Testing;
using Microsoft.Extensions.Hosting;

namespace CommunityToolkit.Aspire.Hosting.Garage.Tests;

[RequiresDocker]
public class GarageFunctionalTestsWithAwsS3Client(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task StorageGetsCreatedAndUsable()
    {
        using var distributedApplicationBuilder = TestDistributedApplicationBuilder.Create(testOutputHelper);

        var passwordParameter = ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(distributedApplicationBuilder,
            $"rootPassword");
        distributedApplicationBuilder.Configuration["Parameters:rootPassword"] = await passwordParameter.GetValueAsync(default);
        var rootPasswordParameter = distributedApplicationBuilder.AddParameter(passwordParameter.Name);

        var garage = distributedApplicationBuilder
            .AddGarageContainer("garage", rootPasswordParameter);

        var garageS3Endpoint = garage.GetEndpoint(GarageContainerResource.S3ApiEndpointName);
        var garageAdminEndpoint = garage.GetEndpoint(GarageContainerResource.AdminEndpointName);

        await using var app = await distributedApplicationBuilder.BuildAsync();

        await app.StartAsync();

        var rns = app.Services.GetRequiredService<ResourceNotificationService>();

        await rns.WaitForResourceHealthyAsync(garage.Resource.Name);

        // Provision an S3 access key via the Garage admin API
        var adminToken = await passwordParameter.GetValueAsync(default);
        var (accessKeyId, secretAccessKey) = await CreateGarageS3Key(garageAdminEndpoint.Port, adminToken!);

        var webApplicationBuilder = Host.CreateApplicationBuilder();

        webApplicationBuilder.Services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = $"http://localhost:{garageS3Endpoint.Port}",
                ForcePathStyle = true,
                AuthenticationRegion = "garage",
            };
            var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
            return new AmazonS3Client(credentials, config);
        });

        using var host = webApplicationBuilder.Build();

        await host.StartAsync();

        var s3Client = host.Services.GetRequiredService<IAmazonS3>();

        await TestApi(s3Client);
    }

    private static async Task<(string AccessKeyId, string SecretAccessKey)> CreateGarageS3Key(int adminPort, string adminToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{adminPort}")
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a new key with createBucket permission
        var createResponse = await httpClient.PostAsJsonAsync("/v2/CreateKey", new
        {
            name = "test-key",
            allow = new { createBucket = true },
        });
        createResponse.EnsureSuccessStatusCode();

        var keyDoc = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessKeyId = keyDoc.GetProperty("accessKeyId").GetString()!;
        var secretAccessKey = keyDoc.GetProperty("secretAccessKey").GetString()!;

        return (accessKeyId, secretAccessKey);
    }

    private static async Task TestApi(IAmazonS3 s3Client)
    {
        const string bucketName = "test-bucket";
        const string objectKey = "test-objects/greeting.txt";
        const string contentType = "text/plain";
        var content = "Hey, I'm using AWS SDK S3 client! It's awesome!"u8.ToArray();

        // Create bucket
        await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });

        // Verify bucket exists
        var listBucketsResponse = await s3Client.ListBucketsAsync();
        Assert.Contains(listBucketsResponse.Buckets, b => b.BucketName == bucketName);

        // Upload object
        using (var uploadStream = new MemoryStream(content))
        {
            await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = uploadStream,
                ContentType = contentType,
                UseChunkEncoding = false,
            });
        }

        // Verify metadata
        var meta = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
        {
            BucketName = bucketName,
            Key = objectKey,
        });
        Assert.Equal(contentType, meta.Headers.ContentType);
        Assert.Equal(content.Length, meta.Headers.ContentLength);

        // Download and verify content
        using var getResponse = await s3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
        });
        using var downloadStream = new MemoryStream();
        await getResponse.ResponseStream.CopyToAsync(downloadStream);
        Assert.Equal(content, downloadStream.ToArray());

        // List objects in bucket
        var listObjectsResponse = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = "test-objects/",
        });
        Assert.Single(listObjectsResponse.S3Objects);
        Assert.Equal(objectKey, listObjectsResponse.S3Objects[0].Key);

        // Delete object and verify removal
        await s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
        });

        var listAfterDelete = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = bucketName,
        });
        Assert.Null(listAfterDelete.S3Objects);
    }
}
