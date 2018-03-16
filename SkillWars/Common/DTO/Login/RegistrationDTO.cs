using Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Common.DTO.Login
{
    public class RegistrationDTO
    {
        /// <summary>
        /// Should be smt like ----@---.--
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Min length 1 symbol
        /// </summary>
        [Required]
        [MinLength(1)]
        public string NickName { get; set; }

        /// <summary>
        /// Min length 6 symbols
        /// </summary>
        [Required]
        [MinLength(6)]        
        public string Password { get; set; }

        /// <summary>
        /// Should be the same as password
        /// </summary>
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public Languages Language { get; set; }

    }
}
