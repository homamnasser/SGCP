using SGCP.Models;
using SGCP.Models.Enums;

namespace SGCP.DTOs.Responses
{
    public class AuthResponseDto
    {

        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? Token { get; set; }

        public RoleResponseDto? Role { get; set; }

        public bool? IsActive { get; set; } = true;

        public GovernmentResponseDto? Government { get; set; }

    }
}
