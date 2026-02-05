namespace SGCP.Models
{
    public class OtpCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; } = null!;
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; }
    }
}

