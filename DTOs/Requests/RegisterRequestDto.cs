
using System.ComponentModel.DataAnnotations;

namespace SGCP.DTOs.Requests
{
    public class RegisterRequestDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(10, ErrorMessage = "Phone number must be 10 digits")]
        public required string Phone { get; set; }
        public required string Password { get; set; }
    }
}
