using Common.Attributes;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Login
{
    public class SteamRegistrationDTO
    {
        /// <summary>
        /// Should be smt like ----@---.--
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    
        [Required]
        public string SteamId { get; set; }

        [Language]
        public Languages Language { get; set; }
    }
}
