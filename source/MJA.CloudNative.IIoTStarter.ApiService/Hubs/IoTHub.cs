using Microsoft.AspNetCore.SignalR;

namespace MJA.CloudNative.IIoTStarter.ApiService.Hubs
{
    public class IoTHub : Hub
    {
        public async Task SendMessage(string user, string message)
            => await Clients.All.SendAsync("ReceiveMqttMessage", user, message);
    }
}
