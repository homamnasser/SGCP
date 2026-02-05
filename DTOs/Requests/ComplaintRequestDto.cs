namespace SGCP.DTOs.Requests
{
    public class ComplaintRequestDto
    {
        public int GovernmentId { get; set; }
        public int TypeId { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        

        public List<IFormFile>? Attachments { get; set; }
    }
}
