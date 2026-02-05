namespace SGCP.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public string Action { get; set; } = null!;

        public string Entity { get; set; } = null!;

        public int? EntityId { get; set; }
        public string Description { get; set; } = null!;

       

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
