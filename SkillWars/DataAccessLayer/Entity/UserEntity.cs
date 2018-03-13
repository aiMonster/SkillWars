using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entity
{
    public class UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SteamId { get; set; }

        public string NickName { get; set; }
        public string Password { get; set; }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }
        public string IsPhoneNumberConfirmed { get; set; }

        public int Balance { get; set; }

        public DateTime RegistrationDate { get; set; }

        public UserEntity() { }

    }
}
