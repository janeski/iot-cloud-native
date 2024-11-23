
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

var apiService = builder.AddProject<Projects.MJA_CloudNative_IIoTStarter_ApiService>("apiservice")
    .WithReference(iotdb)
    .WaitFor(iotdb);
    

//builder.AddProject<Projects.MJA_CloudNative_IIoTStarter_Web>("webfrontend")
//    .WithExternalHttpEndpoints()
//    .WithReference(cache)
//    .WaitFor(cache)
//    .WithReference(apiService)
//    .WaitFor(apiService);

builder.Build().Run();
