using System.ComponentModel.DataAnnotations;

namespace Common.DTO.Lobbie
{
    public class LobbieRequest
    {
        [Required]
        public string Map { get; set; } 
        
        [Range(0, 100)]
        public int Bet { get; set; }

        [Range(2,10)]
        public int AmountPlayers { get; set; }

        [Range(1, 100)]
        public int ExpectingMinutes { get; set; }

        public bool IsPrivate { get; set; }
        public string Password { get; set; }
    }
}
