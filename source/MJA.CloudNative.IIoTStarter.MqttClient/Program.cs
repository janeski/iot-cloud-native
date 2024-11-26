using System.Text;
using System.Text.Json;
using MJA.CloudNative.IIoTStarter.MqttClient.Models;
using MQTTnet;
using MQTTnet.Client;
using Npgsql;


var iotdb = new NpgsqlConnection("iotdb");

var mqttFactory = new MqttFactory();
var mqttClient = mqttFactory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithClientId("IoTListener")
    .WithTcpServer("127.0.0.1", 1883) // Corrected broker details
    .WithCleanSession()
    .Build();

// Handle connection
mqttClient.ApplicationMessageReceivedAsync += async e =>
{
Console.WriteLine("Received Message:");
Console.WriteLine($"Topic: {e.ApplicationMessage.Topic}");
Console.WriteLine($"Payload: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
Console.WriteLine($"QoS: {e.ApplicationMessage.QualityOfServiceLevel}");
Console.WriteLine();

var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
var smartMeterMeasurement = JsonSerializer.Deserialize<SmartMeterMeasurement>(payload);

await iotdb.OpenAsync();

using (var cmd = new NpgsqlCommand("INSERT INTO smart_meter_measurement (id, smartmetername, time, measurement) VALUES (@id, @smartMeterName, @time, @measurement)", iotdb))
{
cmd.Parameters.AddWithValue("id", Guid.NewGuid());
cmd.Parameters.AddWithValue("smartMeterName", smartMeterMeasurement.SmartMeterName);
cmd.Parameters.AddWithValue("time", smartMeterMeasurement.Time);
cmd.Parameters.AddWithValue("measurement", smartMeterMeasurement.Measurement);

await cmd.ExecuteNonQueryAsync();
}
await iotdb.CloseAsync();
};

mqttClient.ConnectedAsync += async (e) =>
{
    Console.WriteLine("Connected to MQTT Broker.");
    // Subscribe to a topic
    string topic = "iot/realtime/SM_001"; // Replace with your topic
    await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
        .WithTopicFilter(topic)
        .Build());
    Console.WriteLine($"Subscribed to topic: {topic}");
};

mqttClient.DisconnectedAsync += e =>
{
    Console.WriteLine("Disconnected from MQTT Broker.");
    return Task.CompletedTask;
};


// Graceful shutdown
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

AppDomain.CurrentDomain.ProcessExit += (s, e) => cts.Cancel();

// Connect to the broker
Console.WriteLine("Connecting to MQTT Broker...");
await mqttClient.ConnectAsync(options, CancellationToken.None);

try
{
    // Wait indefinitely or until cancellation
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
    // Expected when cancellation token is triggered
}

Console.WriteLine("Disconnecting...");
await mqttClient.DisconnectAsync();
