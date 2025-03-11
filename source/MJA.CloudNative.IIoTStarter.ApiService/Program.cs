using MJA.CloudNative.IIoTStarter.ApiService;
using MJA.CloudNative.IIoTStarter.ApiService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSignalR().AddNamedAzureSignalR("signalr");

builder.AddNpgsqlDataSource("iotdb");

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddHostedService<MqttSubscriberService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapHub<IoTHub>("/iot");

app.Run();