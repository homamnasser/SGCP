using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCP.IService;

[ApiController]
[Route("api/Report")]
public class ReportController : ControllerBase
{
    private readonly IComplaintHistoryReportService _reportService;

    public ReportController(IComplaintHistoryReportService reportService)
    {
        _reportService = reportService;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("complaint/{id}/history")]
    public async Task<IActionResult> GetComplaintHistoryPdf(int id)
    {
        try
        {
            var pdfBytes = await _reportService.GeneratePdfAsync(id);
            return File(pdfBytes, "application/pdf", $"ComplaintHistory_{id}.pdf");
        }
        catch (Exception ex)
        {
            return NotFound(new
            {
                message = "Could not generate report",
                error = ex.Message
            });
        }
    }
}
