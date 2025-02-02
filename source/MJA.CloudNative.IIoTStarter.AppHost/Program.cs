var builder = DistributedApplication.CreateBuilder(args);

// MQTT Broker as a Docker container with custom image
var mqtt = builder.AddDockerfile("mqtt", "mqtt/")
    .WithEndpoint(port: 1883, targetPort: 1883, scheme: "tcp", env: "MQTT_SERVER_PORT", name: "mqttBroker");

// a MQTT Broker reference to be used by the API service
var mqttBroker = mqtt.GetEndpoint("mqttBroker");

// SignalR service
var signalr = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureSignalR("signalr")
    : builder.AddConnectionString("signalr");

// IoT database name
var iotdbName = "iotdb";

// PostgreSQL DB Server as a Docker container with custom image with TimescaleDB extension and pgAdmin
var timescale = builder.AddPostgres("timescale")
    .WithEnvironment("POSTGRES_DB", iotdbName)
    .WithBindMount("../MJA.CloudNative.IIoTStarter.ApiService/data/", "/docker-entrypoint-initdb.d")
    .WithImage("timescale/timescaledb", "latest-pg17")
    .WithPgAdmin();

// Add database in the DB server
var iotdb = timescale.AddDatabase(iotdbName);

// API service with custom business logic for handling MQTT messages and minimal API definition
// The API has a reference to the MQTT Broker, DB server, and SignalR service
var api = builder.AddProject<Projects.MJA_CloudNative_IIoTStarter_ApiService>("api")
    .WithReference(iotdb)
    .WithReference(signalr)
    .WithReference(mqttBroker)
    .WaitFor(iotdb)
    .WithExternalHttpEndpoints();

// Web application with Angular front-end with a reference to the API service
builder.AddNpmApp("mja-iot-portal", "../MJA.CloudNative.IIoTStarter.Web")
    .WithReference(api)
    .WithReference(signalr)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
