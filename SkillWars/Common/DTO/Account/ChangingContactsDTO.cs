using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Account
{
    public class ChangingContactsDTO
    {
        public ConactTypes ContactType { get; set; }
        public string AnotherTokenId { get; set; }
        public bool IsOldContactConfirmed { get; set; }
        public bool IsNewContactConfirmed { get; set; }
        public string NewContact { get; set; }
    }
}
