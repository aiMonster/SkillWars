using Common.Entity;
using System;

namespace Common.DTO.Account
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        public NotificationDTO() { }

        public NotificationDTO(NotificationEntity entity)
        {
            Id = entity.Id;
            Text = entity.Text;
            Time = entity.Time;
        }
    }
}
