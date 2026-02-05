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
using SGCP.Models.Enums;
using SGCP.Service;
using System.Net.Mail;
using System.Security.Claims;

namespace SGCP.Controllers
{
    [Route("api/complaint")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintTypeService _complaintTypeService;
        private readonly IGovernmentService _governmentService;
        private readonly IComplaintService _complaintService;
        private readonly IMapper _mapper;
        private readonly IComplaintHistoryService _historyService;
        private readonly IUserService _UserService;
        private readonly DataContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly IAuditAspectService _auditAspectService;
        private readonly IRoundRobinDispatcherService _dispatcher;

        public ComplaintController
            (
            IComplaintTypeService complaintTypeService, IMapper mapper, 
            IGovernmentService governmentService, IComplaintService complaintService,
            IComplaintHistoryService historyService, IUserService userService, DataContext context,
            IAuditLogService auditLogService, IAuditAspectService auditAspectService, IRoundRobinDispatcherService dispatcher
            )
        {
            _complaintTypeService = complaintTypeService;
            _mapper = mapper;
            _governmentService = governmentService;
            _complaintService = complaintService;
            _historyService = historyService;
            _UserService = userService;
            _context = context;
            _auditLogService = auditLogService;
            _auditAspectService = auditAspectService;
            _dispatcher = dispatcher;
        }

        [Authorize(Roles = "User")]
        [HttpPost("create")]



        public async Task<IActionResult> CreateComplaint([FromForm] ComplaintRequestDto request)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

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
                CreatedAt = DateTime.UtcNow,
                Note = "Complaint created"
            };

            var createdComplaint = await _complaintService.CreateComplaint(complaint);
            if (createdComplaint == null)
                return StatusCode(500, "Failed to create complaint");

            var history = new ComplaintHistory
            {
                ComplaintId = createdComplaint.Id,
                EmployeeId = userId,
                GovernmentId = createdComplaint.GovernmentId,
                TypeId = createdComplaint.TypeId,
                Description = createdComplaint.Description,
                Location = createdComplaint.Location,
                Status = createdComplaint.Status,
                ReferenceNumber = createdComplaint.ReferenceNumber,
                Note = "Initial creation",
                CreatedAt = DateTime.UtcNow
            };

            await _historyService.AddAsync(history);

            var complaintFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/complaints"
            );

            if (!Directory.Exists(complaintFolder))
                Directory.CreateDirectory(complaintFolder);

            var attachments = new List<ComplaintAttachment>();

            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var file in request.Attachments)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(complaintFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/complaints/{fileName}";

                    var attachment = new ComplaintAttachment
                    {
                        ComplaintId = createdComplaint.Id,
                        ComplaintHistoryId = history.Id,
                        ImagePath = fileUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _complaintService.AddAttachment(attachment);
                    attachments.Add(attachment);
                }
            }
            await _auditAspectService.LogAsync(
                userId: userId,
                action: "Create",
                entity: "Complaint",
                entityId: createdComplaint.Id,
                description: $"Added attachment to complaint with ReferenceNumber: {createdComplaint.ReferenceNumber}"
            );
            var resDto = _mapper.Map<ComplaintResponseDto>(createdComplaint);
            resDto.Attachments = attachments.Select(a => a.ImagePath).ToList();
            resDto.Status = createdComplaint.Status.ToString();

            resDto.Government = _mapper.Map<GovernmentResponseDto>(
                await _governmentService.GetGovernment(createdComplaint.GovernmentId)
            );

            resDto.Type = _mapper.Map<ComplaintTypeResponseDto>(
                await _complaintTypeService.GetType(createdComplaint.TypeId)
            );

            return Ok(resDto);
        }






        [Authorize(Roles = "User")]
        [HttpGet("myComplaints")]
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
        [HttpGet("{id}/GetComplaint")]

        public async Task<ActionResult<ComplaintTypeResponseDto>> GetComplaint(int id)
        {

            var employeeId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var isActive = await _UserService.IsUserActive(employeeId);
            if (!isActive)
                return Forbid("Your account is deactivated, Please Update password ");

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
        [HttpGet("{number}/GetReferenceNumber")]
        public async Task<ActionResult<ComplaintTypeResponseDto>> GetComplaintByNumber(string number)
        {
            var employeeId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var isActive = await _UserService.IsUserActive(employeeId);
            if (!isActive)
                return Forbid("Your account is deactivated, Please Update password ");



            var complaint = await _complaintService.GetComplaintByReferenceNumber(number);
            if (complaint == null)
                return NotFound("Complaint not found");
            var response = _mapper.Map<ComplaintResponseDto>(complaint);
            response.Status = complaint.Status.ToString();


            var attachments = await _complaintService.GetAttachments(complaint.Id);
            response.Attachments = attachments.Select(a => a.ImagePath).ToList();
            return Ok(response);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/GetComplaintsByGovernment")]
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


        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id}/change-status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromForm] ChangeComplaintStatusRequest request)
        {
            // 1. استخراج معرف الموظف الحالي
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                return Unauthorized("Invalid employee token.");
            }

            var isActive = await _UserService.IsUserActive(employeeId);
            if (!isActive)
                return Forbid("Your account is deactivated, Please Update password");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. منطق جلب الشكوى والتحقق من صلاحية الانتقال والقفل
                var complaint = await _complaintService.GetComplaint(id);


                if (complaint == null)
                    return NotFound("Complaint not found");

                var oldStatus = complaint.Status;
                var newStatus = request.Status;

                if (!IsValidStatusTransition(oldStatus, newStatus))
                    return BadRequest($"Cannot change status from {oldStatus} to {newStatus}");

                var activeLock = await _complaintService.GetActiveLock(complaint.Id);
                if (activeLock != null && activeLock.ExpiresAt > DateTime.UtcNow)
                {
                    if (activeLock.UserId != employeeId)
                    {
                        // شكوى مقفولة لموظف آخر => منع التعديل
                        return Conflict("This complaint is being processed by another employee.");
                    }
                    // إذا كان نفس الموظف صاحب القفل => يسمح له بالتعديل
                }
                if (newStatus == ComplaintStatus.UnderProcessing)
                {
                    var existingLock = await _complaintService.GetActiveLock(complaint.Id);

                    if (existingLock == null)
                    {
                        await _complaintService.LockComplaint(
                            complaint.Id,
                            employeeId,
                            durationMinutes: 1440
                        );
                    }
                }
                else if (newStatus == ComplaintStatus.Completed || newStatus == ComplaintStatus.Rejected)
                {
                    await _complaintService.UnlockComplaint(complaint.Id, employeeId);
                }

                complaint.Status = newStatus;
                await _complaintService.UpdateComplaint(complaint);

                var historyAfter = new ComplaintHistory
                {
                    ComplaintId = complaint.Id,
                    EmployeeId = employeeId,
                    Description = complaint.Description,
                    Location = complaint.Location,
                    TypeId = complaint.TypeId,
                    GovernmentId = complaint.GovernmentId,
                    Status = complaint.Status,
                    ReferenceNumber = complaint.ReferenceNumber,
                    Note = $"Status update to {newStatus} , {request.Note}",
                    CreatedAt = DateTime.UtcNow
                };

                await _historyService.AddAsync(historyAfter);
                await _auditAspectService.LogAsync(
                    userId: employeeId,
                    action: "ChangeStatus",
                    entity: "Complaint",
                    entityId: complaint.Id,
                    description: $"Complaint status changed from {oldStatus} to {newStatus} by employee (UserId={employeeId}). ReferenceNumber: {complaint.ReferenceNumber}"
                );
        /*
                var notificationTitle = $"تحديث حالة الشكوى #{complaint.ReferenceNumber}";

                var notePart = string.IsNullOrWhiteSpace(request.Note)
                    ? ""
                    : $", {request.Note}";

                var notificationBody = $"  حالة شكواك : {newStatus}{notePart}.";
                await _logService.LogNotificationAsync(
                    complaint.UserId,
                    notificationTitle,
                    notificationBody
                );
        */
                //var userFcmToken = await _UserService.GetUserFcmToken(complaint.UserId);
                //if (!string.IsNullOrEmpty(userFcmToken))
                //{
                //    await _notificationService.SendNotificationAsync(
                //        userFcmToken,
                //        notificationTitle,
                //        notificationBody
                //    );
                //}
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Status updated successfully",

                });

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while updating the status.");
            }
        }
        




        private bool IsValidStatusTransition(ComplaintStatus current, ComplaintStatus next)
        {
            return current switch
            {
                ComplaintStatus.New =>
                    next == ComplaintStatus.UnderProcessing,

                ComplaintStatus.UnderProcessing =>
                    next == ComplaintStatus.UnderProcessing ||
                    next == ComplaintStatus.Completed ||
                    next == ComplaintStatus.Rejected,

                _ => false
            };
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/getHistoryByComplaintId")]
        public async Task<IActionResult> GetHistory(int id)
        {

            var history = await _historyService.GetHistoryByComplaintId(id);

            if (history == null || !history.Any())
                return NotFound("Not found history");

            var responseList = new List<ComplaintHistoryResponseDto>();

            foreach (var item in history)
            {
                var dto = _mapper.Map<ComplaintHistoryResponseDto>(item);

                // تحويل attachments لتكون قائمة URLs فقط
                if (item.Attachments != null && item.Attachments.Any())
                {
                    dto.Attachments = item.Attachments
                                        .Select(a => a.ImagePath) // فقط الـ URL
                                        .ToList();
                }
                else
                {
                    dto.Attachments = new List<string>();
                }

                responseList.Add(dto);
            }

            return Ok(responseList);
        }





        [Authorize(Roles = "Employee")]
        [HttpGet("myGovernmentComplaints")]
        public async Task<IActionResult> GetMyGovernmentComplaints()
        {

            var employeeId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var employee = await _UserService.GetUser(employeeId);
            if (employee == null)
                return Unauthorized("User not found");

            var isActive = await _UserService.IsUserActive(employeeId);

            if (!isActive)
                return BadRequest("Your account is deactivated, Please Update password ");

            if (employee.GovernmentId == null)
                return BadRequest("Employee is not assigned to any government");

            var complaints = await _complaintService
                .GetComplaintsByGoverment(employee.GovernmentId.Value);

            var responseList = new List<ComplaintResponseDto>();

            foreach (var complaint in complaints)
            {
                var resDto = new ComplaintResponseDto
                {
                    Id = complaint.Id,
                    Description = complaint.Description,
                    Status = complaint.Status.ToString(),
                    ReferenceNumber = complaint.ReferenceNumber,
                    Note = complaint.Note,
                    Location = complaint.Location,

                    CreatedAt = complaint.CreatedAt

                };

                var attachments = await _complaintService.GetAttachments(complaint.Id);
                resDto.Attachments = attachments.Select(a => a.ImagePath).ToList();

                resDto.Government = _mapper.Map<GovernmentResponseDto>(complaint.Government);

                resDto.Type = _mapper.Map<ComplaintTypeResponseDto>(complaint.Type);

                responseList.Add(resDto);
            }

            return Ok(responseList);
        }


        [Authorize(Roles = "User")]
        [HttpPost("{complaintId}/AddAttachments")]
        public async Task<IActionResult> AddAttachments(int complaintId,[FromForm] List<IFormFile> attachments)
        {

            var complaint = await _complaintService.GetComplaint(complaintId);
            if (complaint == null)
                return NotFound("Complaint not found");

            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var user = await _UserService.GetUser(userId);
            if (user == null)
                return Unauthorized("User not found");

            if (complaint.UserId != userId)
                return Forbid();

            if (complaint.Status == ComplaintStatus.Completed || complaint.Status == ComplaintStatus.Rejected)
                return BadRequest($"Cannot add attachments to {complaint.Status} complaint.");

            var history = new ComplaintHistory
            {
                ComplaintId = complaint.Id,
                EmployeeId = userId,
                Description = complaint.Description,
                Location = complaint.Location,
                TypeId = complaint.TypeId,
                GovernmentId = complaint.GovernmentId,
                Status = complaint.Status,
                ReferenceNumber = complaint.ReferenceNumber,
                Note = "Add attachments",
                CreatedAt = DateTime.UtcNow
            };

            await _historyService.AddAsync(history);

            var complaintFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/complaints"
            );

            if (!Directory.Exists(complaintFolder))
                Directory.CreateDirectory(complaintFolder);

            var attachmentDtos = new List<AttachmentResponseDto>();

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(complaintFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/complaints/{fileName}";

                    var attachmentEntity = new ComplaintAttachment
                    {
                        ComplaintId = complaint.Id,
                        ComplaintHistoryId = history.Id, 
                        ImagePath = fileUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _complaintService.AddAttachment(attachmentEntity);

                    await _auditAspectService.LogAsync(
                 userId: userId,
                 action: "AddAttachments",
                 entity: "Complaint",
                 entityId: complaint.Id,
                 description: $"Added attachment to complaint with ReferenceNumber: {complaint.ReferenceNumber}"
             );

                    attachmentDtos.Add(new AttachmentResponseDto
                    {
                        ImagePath = fileUrl,
                    });
                }
            }

            return Ok(attachmentDtos);
        }

    }
}
