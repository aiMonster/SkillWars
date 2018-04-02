using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Services.WebSockets.Handlers
{
    public class LobbieHandler : WebSocketHandler
    {
        protected readonly ILogger _logger;

        public LobbieHandler(WebSocketConnectionManager webSocketConnectionManager, ILoggerFactory loggerFactory) : base(webSocketConnectionManager)
        {
            _logger = loggerFactory.CreateLogger<LobbieHandler>();
        }

        public override Task OnConnected(WebSocket socket)
        {
            _logger.LogDebug("Connected new user");
            return base.OnConnected(socket);
        }

        public override Task OnDisconnected(WebSocket socket)
        {
            _logger.LogDebug("Disconnected a user");
            return base.OnDisconnected(socket);
        }


        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await SendMessageToAllAsync(message);
        }
    }
}
