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
    [Route("api/[controller]")]
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
    }
}
