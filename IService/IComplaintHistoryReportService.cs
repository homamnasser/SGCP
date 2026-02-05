namespace SGCP.IService
{
    public interface IComplaintHistoryReportService
    {
        Task<byte[]> GeneratePdfAsync(int complaintId);

    }
}
