using SGCP.IService;
using SGCP.Models;

namespace SGCP.Service
{
    public class AuditAspectService : IAuditAspectService
    {
        private readonly IAuditLogService _auditLogService;

        public AuditAspectService(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task LogAsync(int? userId, string action, string entity, int? entityId, string description)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogService.AddAsync(log);
        }
    }
}
