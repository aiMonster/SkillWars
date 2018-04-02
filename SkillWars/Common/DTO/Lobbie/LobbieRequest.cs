using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Lobbie
{
    public class LobbieRequest
    {
        [Required]
        public string Map { get; set; }        
        public int Bet { get; set; }
        public int AmountPlayers { get; set; }
        public int ExpectingMinutes { get; set; }
        public bool IsPrivate { get; set; }
        public string Password { get; set; }
    }
}
