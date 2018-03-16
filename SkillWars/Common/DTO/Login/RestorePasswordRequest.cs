using System.ComponentModel.DataAnnotations;

namespace Common.DTO.Login
{
    public class RestorePasswordRequest
    {
        [Required]
        public string Token { get; set; }

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
    }
}
