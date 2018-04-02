using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Lobbie
{
    public class ParticipatingRequest
    {
        public int LobbieId { get; set; }
        public TeamTypes TeamType { get; set; }
        public string Password { get; set; }
    }
}
