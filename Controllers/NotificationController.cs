using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGCP.IService;
using SGCP.IServices;
using SGCP.Models;
using SGCP.Models.Enums;

namespace SGCP.Controllers
{
  [Route("api/notification")]
  [ApiController]
  public class NotificationController : ControllerBase
  {
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService ;

    public NotificationController(INotificationService notificationService, IUserService userService)
    {
      _notificationService = notificationService;
      _userService = userService;
    }

    public record MessageRequest(string Title, string Body, NotificationType type, int id, List<string> Tokens);
    public record ByIdMessageRequest(int UserId , string Title, string Body, NotificationType type, int id);

    [Authorize(Roles = "Admin")]
    [HttpPost("send")]
    public async Task<IActionResult> SendNotificationAsync([FromForm] MessageRequest request)
    {
      (int, int) response = await _notificationService.SendNotification(request.Title, request.Body, request.type, request.id, request.Tokens);
      return Ok($"[{response.Item1}] Notifications sent Successfully..\n" +
                $"[{response.Item2}] Notifications failed to send!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("send/all")]
    public async Task<IActionResult> SendNotificationToAllUsersAsync([FromForm] MessageRequest request)
    {
      var users = await _userService.GetUsers();

      List<string> tokens = new List<string>();

      foreach(var user in users)
        if (user.FcmToken != null)
          tokens.Add(user.FcmToken);

      (int, int) response = await _notificationService.SendNotification(request.Title, request.Body, request.type, request.id, request.Tokens);
      return Ok($"[{response.Item1}] Notifications sent Successfully..\n" +
                $"[{response.Item2}] Notifications failed to send!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("send/unicast")]
    public async Task<IActionResult> SendNotificationToUserById([FromForm] ByIdMessageRequest request)
    {
      var user = await _userService.GetUser(request.UserId);
      if (user == null)
        return NotFound();

      List<string> tokens = new List<string>();

      var token = user.FcmToken;
      if (token == null)
        return NotFound();

      tokens.Add(token);

      (int, int) response = await _notificationService.SendNotification(request.Title, request.Body, request.type, request.id, tokens);
      return Ok($"[{response.Item1}] Notifications sent Successfully..\n" +
                $"[{response.Item2}] Notifications failed to send!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("send/broadcast")]
    public async Task<IActionResult> SendBroadcastNotificationAsync(string Title, string Body, NotificationType type, int id)
    {
      await _notificationService.SendBroadcastNotification(Title, Body, type, id);

      return Ok("Notifications sent Successfully..");
    }

  }
}
