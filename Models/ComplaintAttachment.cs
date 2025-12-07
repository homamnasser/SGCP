namespace SGCP.Models
{
    public class ComplaintAttachment
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }

        public string ImagePath { get; set; }

    }
}
