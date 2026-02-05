using System.Threading.Tasks;

namespace SGCP.IService
{
    public interface IAuditAspectService
    {
        Task LogAsync(int? userId, string action, string entity, int? entityId, string description);
    }
}
