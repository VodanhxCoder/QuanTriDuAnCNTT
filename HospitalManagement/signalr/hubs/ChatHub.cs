using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace HospitalManagement.signalr.hubs
{
    public class ChatHub : Hub
    {
        private static int _clientCount = 0;

        public override Task OnConnected()
        {
            _clientCount++;
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _clientCount--;
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public static int GetClientCount()
        {
            return _clientCount;
        }

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }
    }
}