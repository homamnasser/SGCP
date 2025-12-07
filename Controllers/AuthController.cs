using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Context;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.Helper;
using SGCP.IService;
using SGCP.Models;
using SGCP.Service;

namespace SGCP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IGovernmentService _governmentService;


        public AuthController(IJwtService jwtService,IUserService userService,IMapper mapper , IGovernmentService governmentService)
        {
            _jwtService = jwtService;
            _userService = userService;
            _mapper = mapper;
            _governmentService = governmentService;

        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromForm] LoginRequestDto request)
        {

            var userAccount = await _userService.GetUserByEmail(request.Email);

            if (userAccount is null || !PasswordHashHandler.VerifyPassword(request.Password, userAccount.Password))
                return null;


            if (userAccount is null)
                return Unauthorized("Invalid credentials");

            var token = await _jwtService.GenerateTokenAsync(userAccount);

            userAccount.Token = token;
            _ = await _userService.UpdateUser(userAccount);


            var resDto = _mapper.Map<AuthResponseDto>(userAccount);


            return Ok(resDto);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromForm] RegisterRequestDto request)
        {
            var existingUserByEmail = await _userService.GetUserByEmail(request.Email);
            var existingUserByPhone = await _userService.GetUserByPhone(request.Phone);

            if (existingUserByEmail is not null)
                return BadRequest("Email already registered");

            if (existingUserByPhone is not null)
                return BadRequest("Phone already registered");


            var newUser = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Password = PasswordHashHandler.HashPassword(request.Password),
                RoleId = 3,
            };

            var created = await _userService.CreateUser(newUser);
            if (!created)
                return StatusCode(500, "Error creating user");

            var user = await _userService.GetUserByEmail(newUser.Email);

            var token = await _jwtService.GenerateTokenAsync(user);
            user.Token = token;
            await _userService.UpdateUser(user);

            var resDto = _mapper.Map<AuthResponseDto>(user);

            return Ok(resDto);
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
                var plainPassword = "SGCP" + new Random().Next(1000, 9999).ToString();

                var user = new User
                {
                    Name = $"{government.Name}{number}",
                    Email = $"{government.Name.ToLower()}{number}@system.local",
                    EmpPassword = plainPassword,
                    Password = PasswordHashHandler.HashPassword(plainPassword),
                    RoleId = 2,
                    GovernmentId = governmentId,
                    IsActive = false

                };

                var created = await _userService.CreateUser(user);
                if (!created)
                    return StatusCode(500, "Error creating user");

                var dbUser = await _userService.GetUserByEmail(user.Email);

                var resDto = _mapper.Map<EmployeeResponseDto>(dbUser);
                resDto.Password = plainPassword;

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
