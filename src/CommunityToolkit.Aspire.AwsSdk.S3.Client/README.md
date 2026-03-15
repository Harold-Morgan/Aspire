# CommunityToolkit.Aspire.AwsSdk.S3.Client

Registers an [IAmazonS3](https://github.com/aws/aws-sdk-net) client in the DI container for connecting to AWS S3 or any S3-compatible object storage service (e.g. [Garage](https://garagehq.deuxfleurs.fr/)).

## Getting started

### Prerequisites

- An AWS S3 bucket **or** an S3-compatible service such as Garage.

### Install the package

```dotnetcli
dotnet add package CommunityToolkit.Aspire.AwsSdk.S3.Client
```

## Usage example

In the _Program.cs_ file of your project, call the `AddAwsS3Client` extension method to register an `IAmazonS3` for use via the dependency injection container.

```csharp
builder.AddAwsS3Client("garage");
```

## Configuration

### Use a connection string

Provide the name of the connection string when calling `builder.AddAwsS3Client()`:

```csharp
builder.AddAwsS3Client("garage");
```

Then add the connection string to the `ConnectionStrings` configuration section:

```json
{
  "ConnectionStrings": {
    "garage": "Endpoint=http://localhost:3900;AccessKey=mykey;SecretKey=mysecret;Region=garage"
  }
}
```

Supported connection string keys:

| Key | Description |
|-----|-------------|
| `Endpoint` | The service URL (required for S3-compatible services) |
| `AccessKey` | The access key ID |
| `SecretKey` | The secret access key |
| `Region`    | The region name (defaults to `us-east-1`) |

### Use configuration providers

Settings can also be bound from the `Aspire:AwsSdk:S3:Client` section (overridable via `configurationSectionName`):

```json
{
  "Aspire": {
    "AwsSdk": {
      "S3": {
        "Client": {
          "ServiceUrl": "http://localhost:3900",
          "Credentials": {
            "AccessKey": "mykey",
            "SecretKey": "mysecret"
          },
          "Region": "garage",
          "ForcePathStyle": true
        }
      }
    }
  }
}
```

### Use inline delegates

```csharp
builder.AddAwsS3Client("garage", configureSettings: settings =>
{
    settings.Credentials = new AwsS3Credentials { AccessKey = "mykey", SecretKey = "mysecret" };
});
```

## AppHost extensions

In your AppHost project, install the `CommunityToolkit.Aspire.Hosting.Garage` library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package CommunityToolkit.Aspire.Hosting.Garage
```

Then, in the _Program.cs_ file of `AppHost`, register a Garage container and pass its reference to downstream projects:

```csharp
var garage = builder.AddGarageContainer("garage");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(garage);
```

The `WithReference` method configures a connection string named `garage` in `MyService`.
In the _Program.cs_ file of `MyService`, consume it with:

```csharp
builder.AddAwsS3Client("garage");
```

Then inject `IAmazonS3` wherever you need it:

```csharp
public class MyService(IAmazonS3 s3Client)
{
    // ...
}
```

## Additional documentation

- https://github.com/aws/aws-sdk-net
- https://docs.aws.amazon.com/AmazonS3/latest/userguide/Welcome.html

## Feedback & contributing

https://github.com/CommunityToolkit/Aspire
