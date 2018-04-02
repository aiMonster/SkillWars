using Common.DTO.Account;

namespace Common.DTO.Lobbie
{
    public class TeamChangedSRequest
    {
        public int LobbieId { get; set; }
        public UserInfo User { get; set; }
    }
}
