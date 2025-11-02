using CommunityToolkit.Aspire.Hosting.ClickHouse;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddClickHouseServer("clickhouse");

builder.AddProject<CommunityToolkit_Aspire_Hosting_ClickHouse_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
