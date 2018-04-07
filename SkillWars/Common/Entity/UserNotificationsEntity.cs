
namespace Common.Entity
{
    public class UserNotificationsEntity
    {
        public int UserId { get; set; }
        public UserEntity User { get; set; }

        public int NotificationId { get; set; }
        public NotificationEntity Notification { get; set; }
    }
}
