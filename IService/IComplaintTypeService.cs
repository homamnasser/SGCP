using SGCP.Models;

namespace SGCP.IService
{
    public interface IComplaintTypeService
    {
        ICollection<ComplaintType> GetTypes();
        Task<ComplaintType?> GetType(int id);
        Task<bool> TypeExists(int id);

        Task<bool> TypeExists(string name);

        Task<bool> CreateType(ComplaintType type);
        Task<bool> UpdateType(ComplaintType type);
        Task<bool> DeleteType(ComplaintType type);

        Task<ICollection<Complaint>> GetComplaintsByType(int typeID);

        Task<bool> Save();
    }
}
