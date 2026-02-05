namespace SGCP.DTOs.Responses
{
  public sealed class VerifywayResponse
  {
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Raw { get; set; }                     // raw response body (for logs)
    public string? Error { get; set; }
  }
}
