# CommunityToolkit.Aspire.Hosting.Garage library

Provides extension methods and resource definitions for the Aspire AppHost to support running [Garage](https://garagehq.deuxfleurs.fr/) containers.

Garage is a self-hosted, S3-compatible distributed object storage server.

## Getting Started

### Install the package

In your AppHost project, install the package using the following command:

```dotnetcli
dotnet add package CommunityToolkit.Aspire.Hosting.Garage
```

### Example usage

Then, in the _Program.cs_ file of `AppHost`, add a Garage resource and consume the connection using the following methods:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var garage = builder.AddGarageContainer("garage");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(garage);

builder.Build().Run();
```

The connection string exposed to dependent services has the format `Endpoint=http://<host>:<port>`. AccessKey and SecretKey must be provisioned after startup via the Garage admin API or CLI.

## Additional Information

https://github.com/CommunityToolkit/Aspire

## Feedback & contributing

https://github.com/CommunityToolkit/Aspire
