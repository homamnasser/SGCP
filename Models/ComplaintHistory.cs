using SGCP.Models.Enums;

namespace SGCP.Models
{
    public class ComplaintHistory
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }
        public int EmployeeId { get; set; }
        public User Employee { get; set; }

        public int GovernmentId { get; set; }
        public Government Government { get; set; }

        public int TypeId { get; set; }
        public ComplaintType Type { get; set; }

        public string Description { get; set; }
        public string? Location { get; set; }

        public ComplaintStatus Status { get; set; }

        public string ReferenceNumber { get; set; }

        public string? Note { get; set; }
        public ICollection<ComplaintAttachment> Attachments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
