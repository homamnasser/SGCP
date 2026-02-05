using SGCP.Models.Enums;
using FirebaseAdmin.Messaging;

namespace SGCP.IServices
{
  public interface INotificationService
  {
    Task<(int, int)> SendNotification(string title, string body, NotificationType type, int id, List<string> tokens);
    Task SendBroadcastNotification(string title, string body, NotificationType type, int id);
  }
}
