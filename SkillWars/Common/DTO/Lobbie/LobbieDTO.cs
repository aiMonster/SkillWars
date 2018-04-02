using Common.Entity;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Lobbie
{
    public class LobbieDTO
    {
        public int Id { get; set; }
        public string Map { get; set; }
        public double Bet { get; set; }
        public int AmountPlayers { get; set; }
        public LobbieStatusTypes Status { get; set; }
        public DateTime StartingTime { get; set; }
        public bool IsPrivate { get; set; }

        public List<TeamDTO> Teams { get; set; } = new List<TeamDTO>();

        public LobbieDTO() { }

        public LobbieDTO(LobbieEntity entity)
        {
            Id = entity.Id;
            Map = entity.Map;
            Bet = entity.Bet;
            AmountPlayers = entity.AmountPlayers;
            Status = entity.Status;
            StartingTime = entity.StartingTime;
            IsPrivate = entity.IsPrivate;

            Teams = entity.Teams.Select(t => new TeamDTO(t)).ToList();
        }
    }
}
