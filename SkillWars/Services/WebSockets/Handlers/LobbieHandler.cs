using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _logger.LogCritical("Really new user");
            return base.OnConnected(socket);
        }

        public override Task OnDisconnected(WebSocket socket)
        {
            _logger.LogCritical("Really lost a user");
            return base.OnDisconnected(socket);
        }



        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await SendMessageToAllAsync(message);
        }
    }
}
