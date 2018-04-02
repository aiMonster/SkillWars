using Common.DTO.Account;
using Common.Entity;
using Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Common.DTO.Lobbie
{
    public class TeamDTO
    {
        public int Id { get; set; }
        public TeamTypes Type { get; set; }

        public List<UserInfo> Users { get; set; } = new List<UserInfo>();

        public TeamDTO() { }

        public TeamDTO(TeamEntity entity)
        {
            Id = entity.Id;
            Type = entity.Type;
            Users = entity.Users.Select(u => new UserInfo(u)).ToList();
        }
    }
}
