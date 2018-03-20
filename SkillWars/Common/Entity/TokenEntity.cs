using System;

namespace Common.Entity
{
    public class TokenEntity
    {
        public string Id { get; set; }

        public UserEntity User { get; set; }
        public int UserId { get; set; }

        public DateTime ExpirationDate { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
