using SGCP.Models;
using SGCP.Models.Enums;

namespace SGCP.DTOs.Responses
{
    public class ComplaintHistoryResponseDto
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public string ReferenceNumber { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Location { get; set; }
        public string? Note { get; set; }

        public ComplaintStatus Status { get; set; }
        public List<string> Attachments { get; set; }

        public GovernmentResponseDto Government { get; set; } = null!;
        public ComplaintTypeResponseDto Type { get; set; } = null!;
        public EmployeeResponseDto Employee { get; set; } = null!;

        public DateTime CreatedAt { get; set; }


    }


}

