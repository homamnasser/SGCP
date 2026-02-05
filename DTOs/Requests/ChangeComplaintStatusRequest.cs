using SGCP.Models.Enums;

namespace SGCP.DTOs.Requests
{
    public class ChangeComplaintStatusRequest
    {
        public ComplaintStatus Status { get; set; }
        public string? Note { get; set; }

    }

}
