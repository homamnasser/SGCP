using SGCP.Models;
using SGCP.Models.Enums;

namespace SGCP.IService
{
    public interface IComplaintHistoryService
    {
        Task AddAsync(ComplaintHistory history);
        Task AddBeforeAndAfterAsync(
            Complaint complaint,
            int employeeId
           
        );
        Task<ICollection<ComplaintHistory>> GetHistoryByComplaintId(int complaintId);

    }
}
