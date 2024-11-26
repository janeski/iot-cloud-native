using MJA.CloudNative.IIoTStarter.ApiService;
using MJA.CloudNative.IIoTStarter.ApiService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddSignalR().AddNamedAzureSignalR("signalr");

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.AddNpgsqlDataSource("iotdb");

builder.Services.AddHostedService<MqttSubscriberService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHub<IoTHub>("/iot");

app.Run();