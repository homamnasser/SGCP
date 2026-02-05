using SGCP.Models.Enums;

namespace SGCP.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int GovernmentId { get; set; }
        public Government Government { get; set; }

        public int TypeId { get; set; }
        public ComplaintType Type { get; set; }

        public string Description { get; set; }
        public string? Location { get; set; }

        public ComplaintStatus Status { get; set; } 

        public string ReferenceNumber { get; set; }
        public string Note { get; set; }



        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ComplaintAttachment> Attachments { get; set; }
        public ICollection<ComplaintHistory> History { get; set; }
    }
}
