using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.Helper;
using SGCP.IService;
using SGCP.IServices;
using SGCP.Models;
using SGCP.Service;
using SGCP.Services;
using System.ComponentModel.DataAnnotations;

namespace SGCP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        IVerifywayService _verifywayService;
        private readonly IMapper _mapper;
        private readonly IGovernmentService _governmentService;
        private readonly IAuditLogService _auditLogService;
        private readonly IAuditAspectService _auditAspectService;

        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 1;

        public AuthController(IJwtService jwtService,IUserService userService,IMapper mapper , IGovernmentService governmentService, IAuditLogService auditLogService , IVerifywayService verifywayService, IAuditAspectService auditAspectService)
        {
            _jwtService = jwtService;
            _userService = userService;
            _verifywayService = verifywayService;
            _mapper = mapper;
            _governmentService = governmentService;
            _auditLogService = auditLogService;
            _auditAspectService = auditAspectService;


        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromForm] LoginRequestDto request)
        {
            var userAccount = await _userService.GetUserByEmail(request.Email);

            if (userAccount == null)
            {
                await _auditAspectService.LogAsync(
                    userId: null,
                    action: "LoginFailed",
                    entity: "User",
                    entityId: null,
                    description: $"Failed login attempt for non-existing email: {request.Email}"
                );

                return Unauthorized("Invalid email or password");
            }

            // تحقق من القفل المؤقت
            if (userAccount.LockoutEnd.HasValue && userAccount.LockoutEnd.Value > DateTime.UtcNow)
            {
                return Unauthorized($"Account is temporarily locked. Try again at {userAccount.LockoutEnd.Value} UTC");
            }

            var isPasswordValid = PasswordHashHandler.VerifyPassword(request.Password, userAccount.Password);

            if (!isPasswordValid)
            {
                // زيادة عداد المحاولات الفاشلة
                userAccount.FailedLoginAttempts++;

                // قفل الحساب إذا تجاوز الحد
                if (userAccount.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    userAccount.LockoutEnd = DateTime.UtcNow.AddMinutes(LOCKOUT_MINUTES);
                    await _userService.UpdateUser(userAccount);

                    await _auditAspectService.LogAsync(
                        userId: userAccount.Id,
                        action: "AccountLocked",
                        entity: "User",
                        entityId: userAccount.Id,
                        description: $"User account temporarily locked due to {MAX_FAILED_ATTEMPTS} failed login attempts."
                    );

                    return Unauthorized($"Account locked due to too many failed login attempts. Try again after {LOCKOUT_MINUTES} minutes.");
                }

                await _userService.UpdateUser(userAccount);

                await _auditAspectService.LogAsync(
                    userId: userAccount.Id,
                    action: "LoginFailed",
                    entity: "User",
                    entityId: userAccount.Id,
                    description: $"Failed login attempt for email: {request.Email}"
                );

                return Unauthorized("Invalid email or password");
            }

            // إعادة تعيين عداد المحاولات بعد تسجيل الدخول الناجح
            userAccount.FailedLoginAttempts = 0;
            userAccount.LockoutEnd = null;
            var token = await _jwtService.GenerateTokenAsync(userAccount);
            userAccount.Token = token;
            await _userService.UpdateUser(userAccount);

            await _auditAspectService.LogAsync(
                userId: userAccount.Id,
                action: "LoginSuccess",
                entity: "User",
                entityId: userAccount.Id,
                description: $"User logged in successfully with email: {request.Email}"
            );

            var resDto = _mapper.Map<AuthResponseDto>(userAccount);
            return Ok(resDto);
        }


        [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromForm] RegisterRequestDto request, CancellationToken ct)
    {
      var existingUserByEmail = await _userService.GetUserByEmail(request.Email);
      var existingUserByPhone = await _userService.GetUserByPhone(request.Phone);

      if (existingUserByEmail is not null)
        return BadRequest("Email already registered");

      if (existingUserByPhone is not null)
        return BadRequest("Phone already registered");


      Random random = new Random();
      string otp = random.Next(100000, 1000000).ToString();
      //string otp = "000000";

      if (!_verifywayService.TryToE164(request.Phone, out var e164))
        return BadRequest("يرجى إدخال رقم ذو مفتاح سوري, مثل: 0934567890 or +963934567890.");

      var result = await _verifywayService.SendOtpAsync(e164, otp, ct);
      if (!result.Success)
        return StatusCode(result.StatusCode, new { message = "تعذر ارسال رمز التحقق، الرجاء المحاولة مرة أخرى!", error = result.Error });



      var newUser = new User
      {
        Name = request.Name,
        Email = request.Email,
        Phone = request.Phone,
        Password = PasswordHashHandler.HashPassword(request.Password),
        RoleId = 3,
        OTP = otp
      };

      var created = await _userService.CreateUser(newUser);
      if (!created)
        return StatusCode(500, "Error creating user");

      var resDto = _mapper.Map<AuthResponseDto>(newUser);

      return Ok(new { message = "تم إنشاء حسابك، يرجى التحقق من رمز التأكيد على الواتساب.", to = _verifywayService.MaskPhone(e164), resDto });
    }


    [HttpPost("register/verify")]
    public async Task<IActionResult> RegisterVerify([Required] int userId, [Required][StringLength(6)] string otp)
    {
      var user = await _userService.GetUser(userId);
      if (user == null)
        return NotFound();


      if (user.OTP != otp)
        return BadRequest("رمز التحقق غير صحيح!");

      user.OTP = null;
      user.IsActive = true;

      var token = await _jwtService.GenerateTokenAsync(user);
      user.Token = token;
      await _userService.UpdateUser(user);

      var resDto = _mapper.Map<AuthResponseDto>(user);

      return Ok(new { message = "تم تأكيد هويتك بنجاح.", resDto });
    }

    [HttpPost("update/fcm")]
    public async Task<IActionResult> UpdateFCM([Required] int userId, [Required] string fcmToken)
    {
      var updated = await _userService.UpdateFcmTokenAsync(userId , fcmToken);
      if (!updated) return BadRequest("somthing went wrong !!");
        
      return Ok("done :)");
    }


    [Authorize(Roles = "Admin")]
        [HttpPost("AddEmployee/{governmentId}")]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(int governmentId)
        {

            try
            {

                var government = await _governmentService.GetGovernment(governmentId);
                if (government == null)
                    throw new Exception("Government not found");

                int number = await _governmentService.GetEmployeesCount(governmentId) + 1;

                var user = new User
                {
                    Name = $"{government.Name}{number}",
                    Email = $"{government.Name.ToLower()}{number}@system.local",
                    RoleId = 2,
                    GovernmentId = governmentId,
                    IsActive = false

                };
                var Password = new PasswordHasher<User>();
                user.Password = Password.HashPassword(user, "12345678");
                var created = await _userService.CreateUser(user);
                if (!created)
                    return StatusCode(500, "Error creating user");

                var dbUser = await _userService.GetUserByEmail(user.Email);

                var resDto = _mapper.Map<EmployeeResponseDto>(dbUser);

                return Ok(resDto);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


            [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var userPhone = User.Identity?.Name;
            var user = await _userService.GetUserByPhone(userPhone);

            if (user != null)
            {
                user.Token = null;
                _ = await _userService.UpdateUser(user);
            }

            return Ok(new { message = "Token removed successfully" });
        }
        


    }
}
