using System.Text.Json.Serialization;

namespace SGCP.Models
{
    public class Government
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<User> Employees { get; set; }
        public ICollection<Complaint> Complaints { get; set; }

    }
}
