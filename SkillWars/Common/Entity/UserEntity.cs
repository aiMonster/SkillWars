using Common.DTO.Login;
using Common.Enums;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Entity
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
        public bool IsPhoneNumberConfirmed { get; set; }

        public int Balance { get; set; }
        public Languages Language { get; set; }
        public Roles Role { get; set; }

        public bool IsPasswordSet { get; set; }

        public DateTime RegistrationDate { get; set; }

        public int? TeamId { get; set; }
        public TeamEntity Team { get; set; }
        public List<UserNotificationsEntity> Notifications { get; set; } = new List<UserNotificationsEntity>();

        public UserEntity() { }

        public UserEntity(RegistrationDTO request)
        {
            Email = request.Email;
            NickName = request.NickName;
            RegistrationDate = DateTime.UtcNow;
            Password = SkillWarsEncoder.Encript(request.Password);
            Language = request.Language;
            Role = Roles.User;
            IsPasswordSet = true;
        }

        public UserEntity(string steamId, string nickName)
        {
            SteamId = steamId;
            NickName = nickName;
            RegistrationDate = DateTime.UtcNow;            
            Role = Roles.User;
        }

    }
}
