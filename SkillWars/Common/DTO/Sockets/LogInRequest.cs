using Common.Enums;

namespace Common.DTO.Sockets
{
    public class LogInRequest : BaseMessage
    {
        public int UserId { get; set; }

        public LogInRequest()
        {
            Type = SocketRequestTypes.LogInRequest;
        }
    }
}
