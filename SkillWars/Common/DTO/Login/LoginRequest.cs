using System.ComponentModel.DataAnnotations;

namespace Common.DTO.Login
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Login { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
