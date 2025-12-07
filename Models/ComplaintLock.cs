namespace SGCP.Models
{
    public class ComplaintLock
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime LockedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
