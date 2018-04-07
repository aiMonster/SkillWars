using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Entity
{
    public class NotificationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public List<UserNotificationsEntity> Users { get; set; } = new List<UserNotificationsEntity>();
    }
}
