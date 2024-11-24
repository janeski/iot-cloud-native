
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var mqqtt = builder.AddContainer("mqtt", "eclipse-mosquitto", "latest");

var iotdbName = "iotdb";
var timescale = builder.AddPostgres("timescale")
    .WithEnvironment("POSTGRES_DB", iotdbName)
    .WithBindMount("../MJA.CloudNative.IIoTStarter.ApiService/data/", "/docker-entrypoint-initdb.d")
    .WithImage("timescale/timescaledb", "latest-pg17")
    .WithPgAdmin();

var iotdb = timescale.AddDatabase(iotdbName);

var api = builder.AddProject<Projects.MJA_CloudNative_IIoTStarter_ApiService>("api")
    .WithReference(iotdb)
    .WaitFor(iotdb)
    .WithExternalHttpEndpoints();

builder.AddNpmApp("Web", "../MJA.CloudNative.IIoTStarter.Web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

//builder.AddProject<Projects.MJA_CloudNative_IIoTStarter_Web>("webfrontend")
//    .WithExternalHttpEndpoints()
//    .WithReference(cache)
//    .WaitFor(cache)
//    .WithReference(apiService)
//    .WaitFor(apiService);

builder.Build().Run();
