using SGCP.Models;

namespace SGCP.DTOs.Responses
{
    public class EmployeeResponseDto
    {

        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? Password { get; set; }

        public RoleResponseDto? Role { get; set; }

        public bool? IsActive { get; set; } = true;

        public required GovernmentResponseDto Government { get; set; }


    }
}
