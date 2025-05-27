using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SweetDream.Hubs
{
    public class ProductHub : Hub
    {
        public async Task SendUpdate()
        {
            await Clients.All.SendAsync("ReceiveProductUpdate");
        }
    }
}
