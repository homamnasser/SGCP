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

        public int? GovernmentId { get; set; }
        public Government? Government { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Complaint> Complaints { get; set; }
        public ICollection<ComplaintHistory> ComplaintHistories { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
