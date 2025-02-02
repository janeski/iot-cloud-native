var builder = DistributedApplication.CreateBuilder(args);

var mqtt = builder.AddDockerfile("mqtt", "mqtt/")
    .WithEndpoint(port: 1883, targetPort: 1883, scheme: "tcp", env: "MQTT_SERVER_PORT", name: "mqttBroker");

var mqttBroker = mqtt.GetEndpoint("mqttBroker");

var signalr = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureSignalR("signalr")
    : builder.AddConnectionString("signalr");

var iotdbName = "iotdb";

var timescale = builder.AddPostgres("timescale")
    .WithEnvironment("POSTGRES_DB", iotdbName)
    .WithBindMount("../MJA.CloudNative.IIoTStarter.ApiService/data/", "/docker-entrypoint-initdb.d")
    .WithImage("timescale/timescaledb", "latest-pg17")
    .WithPgAdmin();

var iotdb = timescale.AddDatabase(iotdbName);

var api = builder.AddProject<Projects.MJA_CloudNative_IIoTStarter_ApiService>("api")
    .WithReference(iotdb)
    .WithReference(signalr)
    .WithReference(mqttBroker)
    .WaitFor(iotdb)
    .WithExternalHttpEndpoints();

builder.AddNpmApp("mja-iot-portal", "../MJA.CloudNative.IIoTStarter.Web")
    .WithReference(api)
    .WithReference(signalr)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
