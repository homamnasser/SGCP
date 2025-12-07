using System.ComponentModel.DataAnnotations;

namespace SGCP.DTOs.Requests
{
    public class ComplaintTypeRequestDto
    {
        [Required(ErrorMessage = "Type name is required")]
        public string Name { get; set; }

    }
}
