using Common.DTO.Lobbie;
using Common.Enums;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entity
{
    public class LobbieEntity
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Map { get; set; }

        public double Bet { get; set; }

        public int AmountPlayers { get; set; }

        public LobbieStatusTypes Status { get; set; }

        public DateTime StartingTime { get; set; }        

        public bool IsPrivate { get; set; }

        public string Password { get; set; }

        public List<TeamEntity> Teams { get; set; } = new List<TeamEntity>();

        public LobbieEntity() { }

        public LobbieEntity(LobbieRequest request)
        {
            Map = request.Map;
            Bet = request.Bet;
            AmountPlayers = request.AmountPlayers;
            StartingTime = DateTime.UtcNow.AddMinutes(request.ExpectingMinutes);            
            IsPrivate = request.IsPrivate;

            if (IsPrivate)
            {
                Password = SkillWarsEncoder.Encript(request.Password);
            }

            Status = LobbieStatusTypes.Expecting;            
        }

    }
}
