namespace SGCP.DTOs.Requests
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
