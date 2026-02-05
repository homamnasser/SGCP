using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.Helper;
using SGCP.IService;
using SGCP.Models;
using SGCP.Service;

namespace SGCP.Controllers
{
    [Route("api/Government")]
    [ApiController]
    public class GovernmentController : ControllerBase
    {
        private readonly IGovernmentService _governmentService;
        private readonly IMapper _mapper;

        public GovernmentController(IGovernmentService governmentService , IMapper mapper)
        {
            _governmentService = governmentService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<ActionResult<GovernmentResponseDto>> CreateGovernment([FromForm] GovernmentRequestDto request)
        {

            var existingGovernment = await _governmentService.GovernmentExists(request.Name);

            if (existingGovernment)
                return BadRequest("Government already found");


            var newGovernment = new Government
            {
                Name = request.Name
            };

            var created = await _governmentService.CreateGovernment(newGovernment);
            if (!created)
                return StatusCode(500, "Error creating Government");


            var resDto = _mapper.Map<GovernmentResponseDto>(newGovernment);

            return Ok(resDto);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/update")]
        public async Task<ActionResult<GovernmentResponseDto>> UpdateGovernment(
           int id,
           [FromForm] GovernmentRequestDto request)
        {
            var government = await _governmentService.GetGovernment(id);
            if (government == null)
                return NotFound("Government not found");

            government.Name = request.Name;

            var updated = await _governmentService.UpdateGovernment(government);
            if (!updated)
                return StatusCode(500, "Error updating government");

            var response = _mapper.Map<GovernmentResponseDto>(government);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id}/GetGovernment")]
        public async Task<ActionResult<GovernmentResponseDto>> GetGovernment(int id)
        {
            var government = await _governmentService.GetGovernment(id);
            if (government == null)
                return NotFound("Government not found");

            var response = _mapper.Map<GovernmentResponseDto>(government);
            return Ok(response);
        }
        [Authorize]
        [HttpGet("GetGovernments")]
        public async Task<IActionResult> GetAllGovernments()
        {
            var governments = await _governmentService.GetGovernments();

            if (governments == null || !governments.Any())
                return NotFound("No governments found");

            var response = _mapper.Map<ICollection<GovernmentResponseDto>>(governments);

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/Delete")]
        public async Task<IActionResult> DeleteGovernment(int id)
        {
            var government = await _governmentService.GetGovernment(id);
            if (government == null)
                return NotFound("Government not found");

            var deleted = await _governmentService.DeleteGovernment(government);
            if (!deleted)
                return StatusCode(500, "Error deleting government");

            return Ok("Government deleted successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/employees")]
        public async Task<IActionResult> GetGovernmentEmployees(int id)
        {
            var governmentExist = await _governmentService.GovernmentExists(id);
            if (!governmentExist)
                return NotFound("Government not found");
            var employees = await _governmentService.GetGovernmentEmployees(id);

            if (!employees.Any())
                return NotFound("No employees found for this government");

            var response = _mapper.Map<ICollection<EmployeeResponseDto>>(employees);
            return Ok(response);
        }

    }
}
