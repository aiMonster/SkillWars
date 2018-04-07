using Common.DTO.Sockets;
using Common.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<int, HashSet<string>> _userSessions;

        public LobbieHandler(WebSocketConnectionManager webSocketConnectionManager, ILoggerFactory loggerFactory) : base(webSocketConnectionManager)
        {
            _logger = loggerFactory.CreateLogger<LobbieHandler>();
            _userSessions = new ConcurrentDictionary<int, HashSet<string>>();
        }

        public override Task OnConnected(WebSocket socket)
        {
            _logger.LogDebug("Connected new user");
            return base.OnConnected(socket);
        }

        public override Task OnDisconnected(WebSocket socket)
        {
            _logger.LogDebug("Disconnected a user");
            FindAndRemoveSession(socket);           
            
            return base.OnDisconnected(socket);
        }

        public async Task SendMessageByUserId(int userId, object message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            if(_userSessions.ContainsKey(userId))
            {
                foreach(var socketId in _userSessions.Where(s => s.Key == userId).FirstOrDefault().Value)
                {
                    await SendMessageAsync(socketId, jsonMessage);
                }
            }
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(message);
            if (baseMessage == null)
            {                
                return;
            }

            if(baseMessage.Type == SocketRequestTypes.LogInRequest)
            {
                var request = JsonConvert.DeserializeObject<LogInRequest>(message);
                AddSession(socket, request.UserId);
            }
            //await SendMessageToAllAsync(message);
        }

        private void AddSession(WebSocket socket, int userId)
        {
            if (!_userSessions.ContainsKey(userId))
            {
                _userSessions.TryAdd(userId, new HashSet<string>());                
            }
            _userSessions[userId].Add(WebSocketConnectionManager.GetId(socket));
        }

        private void FindAndRemoveSession(WebSocket socket)
        {
            var toRemove = new List<int>();
            foreach (var item in _userSessions)
            {
                item.Value.Remove(WebSocketConnectionManager.GetId(socket));
                if (item.Value.Count == 0)
                {
                    toRemove.Add(item.Key);
                }
            }
            foreach (var item in toRemove)
            {
                _userSessions.TryRemove(item, out var removed);
            }
        }
    }
}
