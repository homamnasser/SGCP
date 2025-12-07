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

        // إنشاء حكومة جديدة
        public async Task<bool> CreateGovernment(Government government)
        {
            _context.Governments.Add(government);
            return await Save();
        }

        // حذف حكومة
        public async Task<bool> DeleteGovernment(Government government)
        {
            _context.Governments.Remove(government);
            return await Save();
        }

        // جلب حكومة بالـ Id
        public async Task<Government?> GetGovernment(int id)
        {
            return await _context.Governments
                .Include(g => g.Employees) // إذا عندك علاقة مع User
                .Include(g => g.Complaints) // إذا عندك علاقة مع Complaint
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        // جلب كل الحكومات
        public ICollection<Government> GetGovernments()
        {
            return _context.Governments
                .Include(g => g.Employees)
                .Include(g => g.Complaints)
                .ToList();
        }

        // التحقق من وجود حكومة
        public async Task<bool> GovernmentExists(int id)
        {
            return await _context.Governments.AnyAsync(g => g.Id == id);
        }

        public async Task<bool> GovernmentExists(string name)
        {
            return await _context.Governments.AnyAsync(g => g.Name == name);
        }

        // حفظ التغييرات
        public async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // تحديث حكومة
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
