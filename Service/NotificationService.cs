using SGCP.IServices;
using SGCP.Models.Enums;
using FirebaseAdmin.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGCP.Services.Notifications
{
  public class NotificationService : INotificationService
  {
    public async Task SendBroadcastNotification(string title, string body, NotificationType type, int id)
    {
      var message = new Message()
      {
        Topic = "all",

        Notification = new Notification
        {
          Title = title,
          Body = body,
        },

        Data = new Dictionary<string, string>
        {
          ["Type"] = type.ToString(),
          ["Id"] = id.ToString()
        },

        Android = new AndroidConfig
        {
          Priority = Priority.High,
          TimeToLive = TimeSpan.FromDays(7),
        },

        Apns = new ApnsConfig
        {
          Headers = new Dictionary<string, string>
          {
            ["apns-priority"] = "10"
          },

          Aps = new Aps
          {
            Alert = new ApsAlert { Title = title, Body = body },
            Sound = "default",
            Badge = 1
          }
        },

        Webpush = new WebpushConfig
        {
          Headers = new Dictionary<string, string>
          {
            ["TTL"] = (7 * 24 * 60 * 60).ToString(),
            ["Urgency"] = "high"
          }
        },

      };

      var messaging = FirebaseMessaging.DefaultInstance;
      var response = await messaging.SendAsync(message);
    }

    public async Task<(int, int)> SendNotification(string title, string body, NotificationType type, int id, List<string> tokens)
    {
      var message = new MulticastMessage()
      {
        Tokens = tokens,

        Notification = new Notification
        {
          Title = title,
          Body = body,
        },

        Data = new Dictionary<string, string>
        {
          ["Type"] = type.ToString(),
          ["Id"] = id.ToString()
        },

        Android = new AndroidConfig
        {
          Priority = Priority.High,
          TimeToLive = TimeSpan.FromDays(7),
        },

        Apns = new ApnsConfig
        {
          Headers = new Dictionary<string, string>
          {
            ["apns-priority"] = "10"
          },

          Aps = new Aps
          {
            Alert = new ApsAlert { Title = title, Body = body },
            Sound = "default",
            Badge = 1
          }
        },

        Webpush = new WebpushConfig
        {
          Headers = new Dictionary<string, string> {
            ["TTL"] = (7 * 24 * 60 * 60).ToString(),
            ["Urgency"] = "high"
          }
        },

      };

      var messaging = FirebaseMessaging.DefaultInstance;
      var response = await messaging.SendEachForMulticastAsync(message);

      return (response.SuccessCount, response.FailureCount);
    }

  }
}
