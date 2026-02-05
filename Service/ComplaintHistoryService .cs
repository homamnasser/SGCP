using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.DTOs.Responses;
using SGCP.IService;
using SGCP.Models;
using SGCP.Models.Enums;

namespace SGCP.Service
{
    public class ComplaintHistoryService : IComplaintHistoryService
    {
        private readonly DataContext _context;

        public ComplaintHistoryService(DataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ComplaintHistory history)
        {
            await _context.ComplaintHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<ComplaintHistory>> GetHistoryByComplaintId(int complaintId)
        {
            return await _context.ComplaintHistories
        .Where(h => h.ComplaintId == complaintId)
        .Include(h => h.Employee)
        .ThenInclude(e => e.Role)
        .Include(h => h.Employee)
        .ThenInclude(e => e.Government)
        .Include(h => h.Government)
        .Include(h => h.Type)
        .Include(h => h.Attachments)
        .OrderByDescending(h => h.CreatedAt)
        .ToListAsync();
        }
        public async Task AddBeforeAndAfterAsync(
           Complaint complaint,
           int employeeId
           
          
                     )
        {
            var before = CreateHistoryFromComplaint(
                complaint,
                employeeId
               
            );

            await _context.ComplaintHistories.AddAsync(before);


            var after = CreateHistoryFromComplaint(
                complaint,
                employeeId
               
            );

            await _context.ComplaintHistories.AddAsync(after);

            await _context.SaveChangesAsync();
        }

        private ComplaintHistory CreateHistoryFromComplaint(Complaint complaint, int employeeId)
        {
            return new ComplaintHistory
            {
                ComplaintId = complaint.Id,
                EmployeeId = employeeId,
                
                Status = complaint.Status,
                GovernmentId = complaint.GovernmentId,
                Government = complaint.Government, // تحميل الحكومة
                TypeId = complaint.TypeId,
                Type = complaint.Type, // تحميل نوع الشكوى
                Description = complaint.Description,
                Location = complaint.Location,
                ReferenceNumber = complaint.ReferenceNumber,
                Note = complaint.Note,
                Attachments = complaint.Attachments != null ? complaint.Attachments.ToList() : new List<ComplaintAttachment>(),
                CreatedAt = DateTime.UtcNow
            };
        }


    }

}
