using SGCP.Models;

namespace SGCP.IService
{
    public interface IAuditLogService
    {
        Task AddAsync(AuditLog log);
        Task<AuditLog?> GetByIdAsync(int id);

        Task<ICollection<AuditLog>> GetAllAsync();

        Task<ICollection<AuditLog>> GetByUserIdAsync(int userId);

        Task<ICollection<AuditLog>> GetByEntityAsync(string entity, int? entityId);
    }
}
