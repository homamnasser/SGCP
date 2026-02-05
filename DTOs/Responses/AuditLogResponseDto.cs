namespace SGCP.DTOs.Responses
{
    public class AuditLogResponseDto
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }

        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public int? EntityId { get; set; }
        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
