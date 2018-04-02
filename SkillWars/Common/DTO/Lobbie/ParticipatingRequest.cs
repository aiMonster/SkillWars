using Common.Enums;

namespace Common.DTO.Lobbie
{
    public class ParticipatingRequest
    {
        public int LobbieId { get; set; }
        public TeamTypes TeamType { get; set; }
        public string Password { get; set; }
    }
}
