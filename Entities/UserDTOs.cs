
using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs
{


    public class RegisterUserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

    }
    public class LoginUserDto
    {

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
    }

}
