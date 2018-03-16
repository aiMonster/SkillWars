using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entity
{
    public class TokenEntity
    {
        public string Id { get; set; }

        public UserEntity User { get; set; }
        public int UserId { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
