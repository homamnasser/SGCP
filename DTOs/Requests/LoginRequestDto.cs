using System.ComponentModel.DataAnnotations;

namespace SGCP.DTOs.Requests
{
  public class LoginRequestDto
  {
    [Required(ErrorMessage = "Email number is required")]
    //[StringLength(10, ErrorMessage = "Email number must be 10 digits")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
  }
}
