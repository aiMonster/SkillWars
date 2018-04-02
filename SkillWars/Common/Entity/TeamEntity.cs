using Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Entity
{
    public class TeamEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int LobbieId { get; set; }
        public LobbieEntity Lobbie { get; set; }

        public TeamTypes Type { get; set; }
        public List<UserEntity> Users { get; set; } = new List<UserEntity>();
    }
}
