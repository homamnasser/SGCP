using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.IService;
using SGCP.Models;

namespace SGCP.Service
{
    public class GovernmentService : IGovernmentService
    {
        private readonly DataContext _context;

        public GovernmentService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateGovernment(Government government)
        {
            _context.Governments.Add(government);
            return await Save();
        }

        public async Task<bool> DeleteGovernment(Government government)
        {
            _context.Governments.Remove(government);
            return await Save();
        }

        public async Task<Government?> GetGovernment(int id)
        {
            return await _context.Governments
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<ICollection<Government>> GetGovernments()
        {
            return await _context.Governments
               
                .ToListAsync();
        }

        public async Task<bool> GovernmentExists(int id)
        {
            return await _context.Governments.AnyAsync(g => g.Id == id);
        }

        public async Task<bool> GovernmentExists(string name)
        {
            return await _context.Governments.AnyAsync(g => g.Name == name);
        }

        public async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateGovernment(Government government)
        {
            _context.Governments.Update(government);
            return await Save();
        }

        public async Task<int> GetEmployeesCount(int governmentId)
        {
            var government = await _context.Governments
                .Include(g => g.Employees)
                .FirstOrDefaultAsync(g => g.Id == governmentId);

            return government?.Employees.Count ?? 0;
        }


        public async Task<ICollection<User>> GetGovernmentEmployees(int governmentId)
        {
            var government = await _context.Governments
                .Include(g => g.Employees)
                .ThenInclude(e => e.Role)
                .FirstOrDefaultAsync(g => g.Id == governmentId);

            return government?.Employees ?? new List<User>();
        }

        public async Task<ICollection<Complaint>> GetGovernmentComplaints(int governmentId)
        {
            var government = await _context.Governments
                                           .Include(g => g.Complaints)
                                           .FirstOrDefaultAsync(g => g.Id == governmentId);

            return government?.Complaints ?? new List<Complaint>();
        }
    }
}
