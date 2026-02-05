using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SGCP.Models
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }


        [Required, EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? EmpPassword { get; set; }
        public string Password { get; set; }

        public string? Token { get; set; }

        public int RoleId { get; set; }
        public Role Role  { get; set; }

         public string? OTP { get; set; } = null;


         public int? GovernmentId { get; set; }
        public Government? Government { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }
        public bool IsActive { get; set; } = false;
        public string? FcmToken { get; set; }

        public ICollection<Complaint> Complaints { get; set; }
        public ICollection<ComplaintHistory> ComplaintHistories { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } 

        public ICollection<Notification> Notifications { get; set; }
    }
}
