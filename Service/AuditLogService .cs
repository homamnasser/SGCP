using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.IService;
using SGCP.Models;

namespace SGCP.Service
{
    public class AuditLogService : IAuditLogService
    {
        private readonly DataContext _context;

        public AuditLogService(DataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog log)
        {
            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<ICollection<AuditLog>> GetByUserIdAsync(int userId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<ICollection<AuditLog>> GetByEntityAsync(string entity, int? entityId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.Entity == entity && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs
                .Include(a => a.User)  
                .FirstOrDefaultAsync(a => a.Id == id); 
        }
    }
}
