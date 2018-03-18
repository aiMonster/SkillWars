using Common.Entity;
using Common.Enums;
using System;

namespace Common.DTO.Account
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string SteamId { get; set; }

        public string NickName { get; set; }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }               

        public int Balance { get; set; }
        public Languages Language { get; set; }

        public Roles Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsPassswordSet { get; set; }

        public UserProfile() { }

        public UserProfile(UserEntity entity)
        {
            Id = entity.Id;
            SteamId = entity.SteamId;
            NickName = entity.NickName;
            Email = entity.Email;            
            IsEmailConfirmed = entity.IsEmailConfirmed;
            PhoneNumber = entity.PhoneNumber;
            IsPhoneNumberConfirmed = entity.IsPhoneNumberConfirmed;
            Balance = entity.Balance;
            Language = entity.Language;
            Role = entity.Role;
            RegistrationDate = entity.RegistrationDate;
            IsPassswordSet = entity.IsPasswordSet;
        }
    }
}
