var builder = DistributedApplication.CreateBuilder(args);

builder.AddMinioContainer("minio", "user", "password");

builder.Build().Run();