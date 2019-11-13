using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForLoginDto
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [StringLength(50,MinimumLength=4,ErrorMessage="Password length 4-50 char")]
        public string Password { get; set; }
    }
}