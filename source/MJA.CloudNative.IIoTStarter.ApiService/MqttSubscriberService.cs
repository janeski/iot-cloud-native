
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MJA.CloudNative.IIoTStarter.ApiService.Hubs;
using MJA.CloudNative.IIoTStarter.ApiService.Models;
using MQTTnet;
using MQTTnet.Client;
using Npgsql;

namespace MJA.CloudNative.IIoTStarter.ApiService
{
    public class MqttSubscriberService : BackgroundService
    {
        private readonly ILogger<MqttSubscriberService> _logger;
        private IMqttClient _mqttClient;
        private MqttClientOptions _options;
        private readonly IHubContext<IoTHub> _hubContext;
        private readonly NpgsqlConnection _iotdb;
        private readonly string _mqttBrokerUrl;

        public MqttSubscriberService(IHubContext<IoTHub> hubContext, NpgsqlConnection iotdb, ILogger<MqttSubscriberService> logger, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _iotdb = iotdb;
            _logger = logger;
            var myUri = new Uri(configuration["services:mqtt:mqttBroker:0"]);
            _mqttBrokerUrl = myUri.Host;
            InitializeMqttClient();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _mqttClient.ConnectAsync(_options, stoppingToken);
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _mqttClient.DisconnectAsync();
            await base.StopAsync(stoppingToken);
        }

        private void InitializeMqttClient()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithClientId("IoTClient")
                .WithTcpServer(_mqttBrokerUrl, 1883)
                .WithCleanSession()
                .Build();

            _mqttClient.ConnectedAsync += async e =>
            {
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("iot/realtime/SM_001")
                    .WithAtMostOnceQoS()
                    .Build());

            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _mqttClient.ConnectAsync(_options, CancellationToken.None);
                }
                catch (Exception ex)
                {

                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                var smartMeterMeasurement = JsonSerializer.Deserialize<SmartMeterMeasurement>(payload);
                _logger.LogInformation($"Background MQTT Subscriber got a new message:{smartMeterMeasurement.Measurement} {smartMeterMeasurement.Time}");
                await HandleIncomingMessageAsync("SM_001", smartMeterMeasurement);
            };
        }

        private async Task HandleIncomingMessageAsync(string meter, SmartMeterMeasurement payload)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMqttMessage", meter, payload);

            await _iotdb.OpenAsync();

            using (var cmd = new NpgsqlCommand("INSERT INTO smart_meter_measurement (id, smartmetername, time, measurement) VALUES (@id, @smartMeterName, @time, @measurement)", _iotdb))
            {
                cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("smartMeterName", payload.SmartMeterName);
                cmd.Parameters.AddWithValue("time", payload.Time);
                cmd.Parameters.AddWithValue("measurement", payload.Measurement);

                await cmd.ExecuteNonQueryAsync();
            }
            await _iotdb.CloseAsync();
        }
    }
}
