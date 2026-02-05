using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGCP.IService;
using SGCP.Models;

public class ComplaintHistoryReportService : IComplaintHistoryReportService
{
    private readonly IComplaintHistoryService _historyService;

    public ComplaintHistoryReportService(IComplaintHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task<byte[]> GeneratePdfAsync(int complaintId)
    {
        var histories = await _historyService.GetHistoryByComplaintId(complaintId);

        if (histories == null || !histories.Any())
            throw new Exception("No history found for this complaint.");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("تقرير تاريخ الشكاوى")
                    .FontSize(18)
                    .Bold()
                    .AlignCenter();

                page.Content()
                    .Column(column =>
                    {
                        foreach (var history in histories)
                        {
                            column.Item().BorderBottom(1).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text($"Reference Number: {history.ReferenceNumber}").Bold();
                                c.Item().Text($"Status: {history.Status}");
                                c.Item().Text($"Employee: {history.Employee?.Name ?? "N/A"}");
                                c.Item().Text($"Government: {history.Government?.Name ?? "N/A"}");
                                c.Item().Text($"Type: {history.Type?.Name ?? "N/A"}");
                                c.Item().Text($"Description: {history.Description}");
                                if (!string.IsNullOrWhiteSpace(history.Location))
                                    c.Item().Text($"Location: {history.Location}");
                                if (!string.IsNullOrWhiteSpace(history.Note))
                                    c.Item().Text($"Note: {history.Note}");
                                c.Item().Text($"Created At: {history.CreatedAt:yyyy-MM-dd HH:mm}");
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on: ");
                        x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                    });
            });
        })
        .GeneratePdf();

        return pdfBytes;
    }
}
