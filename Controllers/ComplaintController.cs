using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCP.DTOs.Requests;
using SGCP.DTOs.Responses;
using SGCP.IService;
using SGCP.Models;
using SGCP.Models.Enums;
using SGCP.Service;
using System.Net.Mail;
using System.Security.Claims;

namespace SGCP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintTypeService _complaintTypeService;
        private readonly IGovernmentService _governmentService;
        private readonly IComplaintService _complaintService;
        private readonly IMapper _mapper;

        public ComplaintController(IComplaintTypeService complaintTypeService, IMapper mapper, IGovernmentService governmentService, IComplaintService complaintService)
        {
            _complaintTypeService = complaintTypeService;
            _mapper = mapper;
            _governmentService = governmentService;
            _complaintService = complaintService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Type/Create")]
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
        [HttpPut("Type/Update/{id}")]
        public async Task<ActionResult<ComplaintTypeResponseDto>> UpdateType( int id, [FromForm] ComplaintTypeRequestDto request)
        {
            var existingType = await _complaintTypeService.TypeExists(id);

            if (!existingType )
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



        [Authorize(Roles = "User")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateComplaint([FromForm] ComplaintRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User identity missing");

            var userId = int.Parse(userIdClaim);

            if (!await _governmentService.GovernmentExists(request.GovernmentId))
                return BadRequest("Government not found");

            if (!await _complaintTypeService.TypeExists(request.TypeId))
                return BadRequest("Complaint type not found");

            var reference = $"CMP-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            var complaint = new Complaint
            {
                UserId = userId,
                GovernmentId = request.GovernmentId,
                TypeId = request.TypeId,
                Description = request.Description,
                Location = request.Location,
                Status = ComplaintStatus.New,
                ReferenceNumber = reference,
                CreatedAt = DateTime.UtcNow
            };

            var createdComplaint = await _complaintService.CreateComplaint(complaint);

            if (createdComplaint == null)
                return StatusCode(500, "Failed to create complaint");

            var complaintFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "complaints"
            );

            if (!Directory.Exists(complaintFolder))
                Directory.CreateDirectory(complaintFolder);

            var attachments = new List<ComplaintAttachment>();

            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var image in request.Attachments)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var filePath = Path.Combine(complaintFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await image.CopyToAsync(stream);

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/complaints/{fileName}";

                    var attachment = new ComplaintAttachment
                    {
                        ComplaintId = createdComplaint.Id,
                        ImagePath = fileUrl,
                    };

                    attachments.Add(attachment);
                    await _complaintService.AddAttachment(attachment);
                }
            }

            var resDto = _mapper.Map<ComplaintResponseDto>(createdComplaint);
            resDto.Attachments =  attachments.Select(a => a.ImagePath).ToList();
            resDto.Status = createdComplaint.Status.ToString();

            var government = await _governmentService.GetGovernment(createdComplaint.GovernmentId);
            resDto.Government = _mapper.Map<GovernmentResponseDto>(government);

            var type = await _complaintTypeService.GetType(createdComplaint.TypeId);
            resDto.Type = _mapper.Map<ComplaintTypeResponseDto>(type);

            return Ok(resDto);
        }


        [Authorize(Roles = "User")]
        [HttpGet("my-complaints")]
        public async Task<IActionResult> GetUserComplaints()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User identity missing");

            var userId = int.Parse(userIdClaim);


            var complaints = await _complaintService.GetComplaintsByUser(userId);

            if (complaints == null || !complaints.Any())
                return Ok(new List<ComplaintResponseDto>());

            var responseList = new List<ComplaintResponseDto>();

            foreach (var complaint in complaints)
            {
                var resDto = _mapper.Map<ComplaintResponseDto>(complaint);

                resDto.Status = complaint.Status.ToString();

                var attachments = await _complaintService.GetAttachments(complaint.Id);
                resDto.Attachments = attachments.Select(a => a.ImagePath).ToList();

                var government = await _governmentService.GetGovernment(complaint.GovernmentId);
                resDto.Government = _mapper.Map<GovernmentResponseDto>(government);

                var type = await _complaintTypeService.GetType(complaint.TypeId);
                resDto.Type = _mapper.Map<ComplaintTypeResponseDto>(type);

                responseList.Add(resDto);
            }

            return Ok(responseList);
        }

        [Authorize]
        [HttpGet("GetComplaint/{id}")]

        public async Task<ActionResult<ComplaintTypeResponseDto>> GetComplaint(int id)
        {
            var existingComplaint = await _complaintService.ComplaintExists(id);
            if (!existingComplaint)
                return NotFound("Complaint not found");

            var complaint = await _complaintService.GetComplaint(id);

            var response = _mapper.Map<ComplaintResponseDto>(complaint);
            response.Status = complaint.Status.ToString();


            var attachments = await _complaintService.GetAttachments(complaint.Id);
            response.Attachments = attachments.Select(a => a.ImagePath).ToList();
            return Ok(response);
        }
       
        
        [Authorize]
        [HttpGet("GetComplaintsByGovernment/{id}")]
        public async Task<ActionResult<ComplaintTypeResponseDto>> GetComplaintsByGoverment(int id)
        {
            var existingGovernment = await _governmentService.GovernmentExists(id);
            if (!existingGovernment)
                return NotFound("Government not found");
            var complaints = await _complaintService.GetComplaintsByGoverment(id);
            var responseList = new List<ComplaintResponseDto>();

            foreach (var complaint in complaints)
            {
                var resDto = _mapper.Map<ComplaintResponseDto>(complaint);

                resDto.Status = complaint.Status.ToString();

                var attachments = await _complaintService.GetAttachments(complaint.Id);
                resDto.Attachments = attachments.Select(a => a.ImagePath).ToList();

                var government = await _governmentService.GetGovernment(complaint.GovernmentId);
                resDto.Government = _mapper.Map<GovernmentResponseDto>(government);

                var type = await _complaintTypeService.GetType(complaint.TypeId);
                resDto.Type = _mapper.Map<ComplaintTypeResponseDto>(type);

                responseList.Add(resDto);
            }

            return Ok(responseList);
        }


    }
}
