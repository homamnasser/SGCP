using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.IService;
using SGCP.Models;

namespace SGCP.Service
{
    public class ComplaintService : IComplaintService
    {
        private readonly DataContext _context;

        public ComplaintService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> ComplaintExists(int id)
        {
            return await _context.Complaints.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ComplaintExists(string reference)
        {
            return await _context.Complaints.AnyAsync(c => c.ReferenceNumber == reference);
        }

        public async Task<Complaint> CreateComplaint(Complaint complaint)
        {
            await _context.Complaints.AddAsync(complaint);
            await Save();

            return complaint;
        }

        public async Task<bool> DeleteType(Complaint complaint)
        {
            _context.Complaints.Remove(complaint);
            return await Save();
        }

        public async Task<Complaint?> GetComplaint(int id)
        {
            return await _context.Complaints
                .Include(c => c.Attachments)
                .Include(c => c.History)
                .Include(c => c.User)
                .Include(c => c.Government)
                .Include(c => c.Type)
                .FirstOrDefaultAsync(c => c.Id == id);

        }

        public async Task<Complaint?> GetComplaintByReferenceNumber(string number)
        {
            return await _context.Complaints
                .Include(c => c.Attachments)
                .Include(c => c.History)
                .Include(c => c.User)
                .Include(c => c.Government)
                .Include(c => c.Type)
                .FirstOrDefaultAsync(c => c.ReferenceNumber == number);

        }

        public ICollection<Complaint> GetComplaints()
        {
            return _context.Complaints
                .Include(c => c.Attachments)
                .Include(c => c.User)
                .Include(c => c.Government)
                .Include(c => c.Type)
                .ToList();
        }

        public async Task<ICollection<Complaint>> GetComplaintsByGoverment(int govId)
        {
            return await _context.Complaints
                .Where(c => c.GovernmentId == govId)
                .Include(c => c.Attachments)
                .Include(c => c.User)
                .Include(c => c.Government)
                .Include(c => c.Type)
                .ToListAsync();
        }

        public async Task<ICollection<Complaint>> GetComplaintsByUser(int userId)
        {
            return await _context.Complaints
                .Where(c => c.UserId == userId)
                .Include(c => c.Attachments)
                .Include(c => c.Government)
                .Include(c => c.Type)
                .ToListAsync();
        }

        public async Task<bool> UpdateComplaint(Complaint complaint)
        {
            _context.Complaints.Update(complaint);
            return await Save();
        }


        public async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }



        public async Task<bool> AddAttachment(ComplaintAttachment attachments)
        {
            await _context.ComplaintAttachments.AddAsync(attachments);
            return await Save();

        }

        public async Task<ICollection<ComplaintAttachment>> GetAttachments(int complaintId)
        {
            return await _context.ComplaintAttachments
                .Where(a => a.ComplaintId == complaintId)
                .ToListAsync();
        }
        public async Task<bool> AddHistoryAsync(ComplaintHistory history)
        {
            await _context.ComplaintHistories.AddAsync(history);
            return await Save();
        }
        public async Task<string> LockComplaint(int complaintId, int userId, int durationMinutes = 1440)
        {
            var complaint = await GetComplaint(complaintId);
            if (complaint == null)
                return "Complaint not found";

            var existingLock = await _context.ComplaintLocks
                .FirstOrDefaultAsync(l => l.ComplaintId == complaintId);

            if (existingLock != null)
            {
                if (existingLock.ExpiresAt > DateTime.UtcNow)
                {
                    if (existingLock.UserId != userId)
                    {
                        // Lock نشط لموظف آخر
                        return "This complaint is being processed by another employee.";
                    }
                }
                else
                {
                    // Lock منتهي => إعادة القفل للموظف الحالي
                    existingLock.UserId = userId;
                    existingLock.LockedAt = DateTime.UtcNow;
                    existingLock.ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes);
                }
            }
            else
            {
                // لا يوجد Lock => إنشاء جديد
                var newLock = new ComplaintLock
                {
                    ComplaintId = complaintId,
                    UserId = userId,
                    LockedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes)
                };
                await _context.ComplaintLocks.AddAsync(newLock);
            }

            await Save();
            return "Lock acquired successfully";
        }

        public async Task UnlockComplaint(int complaintId, int userId)
        {
            var existingLock = await _context.ComplaintLocks
                .FirstOrDefaultAsync(l => l.ComplaintId == complaintId && l.UserId == userId);

            if (existingLock != null)
            {
                _context.ComplaintLocks.Remove(existingLock);
                await Save();
            }
        }

        public async Task<ComplaintLock?> GetActiveLock(int complaintId)
        {
            return await _context.ComplaintLocks
                .FirstOrDefaultAsync(l => l.ComplaintId == complaintId && l.ExpiresAt > DateTime.UtcNow);
        }

    }
}
