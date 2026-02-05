using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;

namespace SGCP.IServices
{
  public interface IVerifywayService
  {
    /// <summary>Send a specific OTP code via WhatsApp to a recipient.</summary>
    Task<VerifywayResponse> SendOtpAsync(string phoneE164, string code, CancellationToken ct = default);
    bool TryToE164(string input, out string e164);
    string MaskPhone(string phone);
  }
}