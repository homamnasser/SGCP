using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.IService;
using SGCP.Models;

namespace SGCP.Service
{
    public class ComplaintTypeService : IComplaintTypeService
    {
        private readonly DataContext _context;

        public ComplaintTypeService(DataContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateType(ComplaintType type)
        {
            _context.ComplaintTypes.Add(type);
            return await Save();
        }

        public async Task<bool> DeleteType(ComplaintType type)
        {
            _context.ComplaintTypes.Remove(type);
            return await _context.SaveChangesAsync() > 0;
        }


        public Task<ICollection<Complaint>> GetComplaintsByType(int typeID)
        {
            throw new NotImplementedException();
        }

        public async Task<ComplaintType?> GetType(int id)
        {
            return await _context.ComplaintTypes
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<ICollection<ComplaintType>> GetTypes()
        {
            return await _context.ComplaintTypes
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TypeExists(int id)
        {
            return await _context.ComplaintTypes.AnyAsync(g => g.Id == id);
        }

        public async Task<bool> TypeExists(string name)
        {
            return await _context.ComplaintTypes.AnyAsync(g => g.Name == name);

        }

        public async Task<bool> UpdateType(ComplaintType type)
        {
            _context.ComplaintTypes.Update(type);
            return await Save();
        }
    }
}
