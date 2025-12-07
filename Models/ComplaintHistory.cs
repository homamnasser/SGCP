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

        public ComplaintStatus OldStatus { get; set; }
        public ComplaintStatus NewStatus { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
