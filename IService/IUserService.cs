using SGCP.DTOs.Requests;
using SGCP.Models;

namespace SGCP.IService
{
    public interface IUserService
    {

        Task<ICollection<User>> GetUsers();
        Task<User?> GetUser(int id);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByPhone(string phone);


        Task<bool> UserExists(int id);

        Task<bool> CreateUser(User user);
        Task<bool> UpdateUser(User user);
        Task<bool> DeleteUser(User user);

        Task<ICollection<Complaint>> GetUserComplaints(int userId);
        Task<ICollection<ComplaintHistory>> GetUserComplaintHistories(int userId);
        Task<ICollection<Notification>> GetUserNotifications(int userId);
        Task<bool> IsUserActive(int userId);
        Task<string?> GetUserFcmToken(int userId);
        Task<bool> UpdateFcmTokenAsync(int userId, string fcmToken);

        Task<bool> Save();

    }
}
