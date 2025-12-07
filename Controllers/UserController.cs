using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.Helper;
using SGCP.IService;
using SGCP.Models;
using SGCP.Service;
using System.Security.Claims;

namespace SGCP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {


        private readonly IUserService _userService;
        private readonly IGovernmentService _governmentService;
        private readonly IMapper _mapper;


        public UserController(IUserService userService, IMapper mapper, IGovernmentService governmentService)
        {
            _userService = userService;
            _mapper = mapper;
            _governmentService = governmentService;

        }

        [Authorize]
        [HttpPut("UpdatePassword")]
        public async Task<ActionResult> UpdatePassword([FromForm] string pass)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            User user = await _userService.GetUser(userId);

            user.Password = PasswordHashHandler.HashPassword(pass);
            user.EmpPassword = null;
            user.IsActive = true;

            var updated = await _userService.UpdateUser(user);

            if (!updated)
                return StatusCode(500, "Error updating Password");

            return Ok("Password updated successfuly");
        }

    }
}
