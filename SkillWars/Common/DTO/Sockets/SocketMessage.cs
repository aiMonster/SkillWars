using Common.Enums;

namespace Common.DTO.Sockets
{
    public class SocketMessage<T>
    {
        public NotificationTypes NotificationType { get; set; }
        public T Data { get; set; }

        public SocketMessage() { }

        public SocketMessage(T data)
        {
            Data = data;
        }

        public SocketMessage(T data, NotificationTypes notification)
        {
            Data = data;
            NotificationType = notification;
        }
    }
}
