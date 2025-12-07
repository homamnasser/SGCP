using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.IService;
using SGCP.Models;
using System;

namespace SGCP.Service
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users
                           .Include(u => u.Role)
                           .Include(u => u.Government)
                           .ToList();
        }

        public async Task<User?> GetUser(int id)
        {
            return await _context.Users
                                 .Include(u => u.Role)
                                 .Include(u => u.Government)
                                 .Include(u => u.Complaints)
                                 .Include(u => u.ComplaintHistories)
                                 .Include(u => u.Notifications)
                                 .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                                 .Include(u => u.Role)
                                 .Include(u => u.Government)
                                 .Include(u => u.Complaints)
                                 .Include(u => u.ComplaintHistories)
                                 .Include(u => u.Notifications)
                                 .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByPhone(string phone)
        {
            return await _context.Users
                                 .Include(u => u.Role)
                                 .Include(u => u.Government)
                                 .Include(u => u.Complaints)
                                 .Include(u => u.ComplaintHistories)
                                 .Include(u => u.Notifications)
                                 .FirstOrDefaultAsync(u => u.Phone == phone);
        }


        public async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> CreateUser(User user)
        {
            _context.Users.Add(user);
            return await Save();
        }

        public async Task<bool> UpdateUser(User user)
        {
            _context.Users.Update(user);
            return await Save();
        }

        public async Task<bool> DeleteUser(User user)
        {
            _context.Users.Remove(user);
            return await Save();
        }

        public async Task<ICollection<Complaint>> GetUserComplaints(int userId)
        {
            var user = await _context.Users
                                     .Include(u => u.Complaints)
                                     .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Complaints ?? new List<Complaint>();
        }

        public async Task<ICollection<ComplaintHistory>> GetUserComplaintHistories(int userId)
        {
            var user = await _context.Users
                                     .Include(u => u.ComplaintHistories)
                                     .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.ComplaintHistories ?? new List<ComplaintHistory>();
        }

        public async Task<ICollection<Notification>> GetUserNotifications(int userId)
        {
            var user = await _context.Users
                                     .Include(u => u.Notifications)
                                     .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Notifications ?? new List<Notification>();
        }

        public async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
