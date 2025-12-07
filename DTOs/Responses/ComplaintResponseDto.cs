using SGCP.Models;
using SGCP.Models.Enums;

namespace SGCP.DTOs.Responses
{
    public class ComplaintResponseDto
    {
        public int Id { get; set; }
        public GovernmentResponseDto Government { get; set; }
        public ComplaintTypeResponseDto Type { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Attachments { get; set; }
    }
}
