using Common.DTO.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Lobbie
{
    public class TeamChangedSRequest
    {
        public int LobbieId { get; set; }
        public UserInfo User { get; set; }
    }
}
