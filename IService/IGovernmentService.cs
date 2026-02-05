using SGCP.Models;
using System.Diagnostics.Metrics;

namespace SGCP.IService
{
    public interface IGovernmentService
    {
        Task<ICollection<Government>> GetGovernments();
        Task<Government?> GetGovernment(int id);
        Task<bool> GovernmentExists(int id);

        Task<bool> GovernmentExists(string name);

        Task<bool>  CreateGovernment(Government government);
        Task<bool>  UpdateGovernment(Government government);
        Task<bool>  DeleteGovernment(Government government);

        Task<ICollection<User>> GetGovernmentEmployees(int governmentId);
        Task<ICollection<Complaint>> GetGovernmentComplaints(int governmentId);
        Task<int> GetEmployeesCount(int governmentId);

        Task<bool>  Save();
    }
}
