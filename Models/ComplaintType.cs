using SGCP.Models;

namespace SGCP.Models
{

    public class ComplaintType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Complaint> Complaints { get; set; }
    }
}