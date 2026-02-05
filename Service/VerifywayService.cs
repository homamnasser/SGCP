using Microsoft.Extensions.Options;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.Helper;
using SGCP.IService;
using SGCP.IServices;
using SGCP.Models;
using System.Text;

namespace SGCP.Services
{
  public class VerifywayService : IVerifywayService
  {
    private readonly HttpClient _http;
    private readonly VerifywayOptions _opt;

    public VerifywayService(HttpClient http, IOptions<VerifywayOptions> opt)
    {
      _http = http;
      _opt = opt.Value;
    }

    public async Task<VerifywayResponse> SendOtpAsync(string phoneE164, string code, CancellationToken ct = default)
    {
      // Minimal payload exactly like their cURL (no trailing comma, no extra fields)
      var payload = new
      {
        recipient = phoneE164,    // e.g. "963930882854"
        type = "otp",
        code = code,           // send as string
        channel = "whatsapp",
        lang = "ar",
      };

      // Build the request explicitly so headers are crystal-clear
      var json = System.Text.Json.JsonSerializer.Serialize(payload,
          new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });

      using var req = new HttpRequestMessage(HttpMethod.Post, _http.BaseAddress);
      req.Content = new StringContent(json, Encoding.UTF8, "application/json");
      req.Headers.Accept.Clear();
      req.Headers.Accept.ParseAdd("application/json"); // matches their example

      using var httpRes = await _http.SendAsync(req, ct);
      var body = await httpRes.Content.ReadAsStringAsync(ct);

      return new VerifywayResponse
      {
        Success = httpRes.IsSuccessStatusCode,
        StatusCode = (int)httpRes.StatusCode,
        Raw = body,
        Error = httpRes.IsSuccessStatusCode ? null : $"VerifyWay error {(int)httpRes.StatusCode}: {body}"
      };
    }


    public string MaskPhone(string phone)
    {
      if (phone.Length <= 4) return phone;
      return new string('*', phone.Length - 4) + phone[^4..];
    }


    public bool TryToE164(string input, out string e164)
    {
      e164 = string.Empty;
      if (string.IsNullOrWhiteSpace(input)) return false;

      var digits = new string(input.Where(char.IsDigit).ToArray());

      // Strip leading country code variants
      if (digits.StartsWith("00963")) digits = digits[5..];
      else if (digits.StartsWith("963")) digits = digits[3..];

      // If starts with 0, drop it (domestic format)
      if (digits.StartsWith("0")) digits = digits[1..];

      // Now we expect a mobile starting with 9 and 9 more digits (total 9 digits: 9XXXXXXXX)
      if (digits.Length == 9 && digits.StartsWith("9"))
      {
        e164 = $"963{digits}";
        return true;
      }
      return false; // not a recognized Syrian mobile
    }
  }
}
