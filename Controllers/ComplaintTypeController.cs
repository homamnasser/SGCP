using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.IService;
using SGCP.Models;
using SGCP.Service;

namespace SGCP.Controllers
{
    [Route("api/complaintType")]
    [ApiController]

    public class ComplaintTypeController: ControllerBase
    {
        private readonly IComplaintTypeService _complaintTypeService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public ComplaintTypeController(IComplaintTypeService complaintTypeService, IMapper mapper, IUserService userService)
        {
            _complaintTypeService = complaintTypeService;
            _mapper = mapper;
            _userService = userService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<ActionResult<ComplaintTypeResponseDto>> CreateComplaintType([FromForm] ComplaintTypeRequestDto request)
        {
            var existingType = await _complaintTypeService.TypeExists(request.Name);

            if (existingType)
                return BadRequest("Type already found");

            var newType = new ComplaintType
            {
                Name = request.Name
            };

            var created = await _complaintTypeService.CreateType(newType);
            if (!created)
                return StatusCode(500, "Error creating Type");


            var resDto = _mapper.Map<ComplaintTypeResponseDto>(newType);

            return Ok(resDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Update")]
        public async Task<ActionResult<ComplaintTypeResponseDto>> UpdateType(int id, [FromForm] ComplaintTypeRequestDto request)
        {
            var existingType = await _complaintTypeService.TypeExists(id);
            if (!existingType)
                return NotFound("Type not found");

            var typeWithSameName = await _complaintTypeService.TypeExists(request.Name);
            if (typeWithSameName)
                return BadRequest("Another type with the same name already exists");

            ComplaintType type = await _complaintTypeService.GetType(id);

            type.Name = request.Name;

            var updated = await _complaintTypeService.UpdateType(type);
            if (!updated)
                return StatusCode(500, "Error updating Type");

            var response = _mapper.Map<ComplaintTypeResponseDto>(type);

            return Ok(response);
        }



        [Authorize]
        [HttpGet("{id}/GetType")]

        public async Task<ActionResult<ComplaintTypeResponseDto>> GetComplaintType(int id)
        {
            var employeeId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var isActive = await _userService.IsUserActive(employeeId);
            if (!isActive)
                return Forbid("Your account is deactivated, Please Update password ");

            var existingType = await _complaintTypeService.TypeExists(id);
            if (!existingType)
                return NotFound("Type not found");

            var complaint = await _complaintTypeService.GetType(id);

            var response = _mapper.Map<ComplaintTypeResponseDto>(complaint);
            
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetTypes")]
        public async Task<ActionResult<ICollection<ComplaintTypeResponseDto>>> GetAllTypes()
        {
            var employeeId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var isActive = await _userService.IsUserActive(employeeId);
            if (!isActive)
                return Forbid("Your account is deactivated");

            var types = await _complaintTypeService.GetTypes();

            var response = _mapper.Map<ICollection<ComplaintTypeResponseDto>>(types);

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/Delete")]
        public async Task<IActionResult> DeleteType(int id)
        {
            var existingType = await _complaintTypeService.TypeExists(id);
            if (!existingType)
                return NotFound("Type not found");

            var type = await _complaintTypeService.GetType(id);

            var deleted = await _complaintTypeService.DeleteType(type);
            if (!deleted)
                return StatusCode(500, "Error deleting type");

            return Ok("Type deleted successfully");
        }
    }
}
