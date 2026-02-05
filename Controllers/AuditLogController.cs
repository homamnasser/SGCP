using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCP.DTOs.Responses;
using SGCP.IService;

namespace SGCP.Controllers
{
    [Route("api/Log")]
    [ApiController]
    public class AuditLogController: ControllerBase
    {
        

        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly IRoundRobinDispatcherService _dispatcher;

        public AuditLogController(IUserService userService, IMapper mapper, IAuditLogService auditLogService, IRoundRobinDispatcherService dispatcher)
        {
            _userService = userService;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _dispatcher = dispatcher;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAuditLogs")]
        public async Task<IActionResult> GetAuditLogs()
        {

            var logs = await _auditLogService.GetAllAsync();

            if (logs == null || !logs.Any())
                return NotFound("No audit logs found");

            var responseList = new List<AuditLogResponseDto>();

            foreach (var log in logs)
            {
                var dto = new AuditLogResponseDto
                {
                    Id = log.Id,
                    UserId = log.UserId,
                    UserName = log.User != null ? log.User.Name : null,
                    UserEmail= log.User != null ? log.User.Email : null,
                    Action = log.Action,
                    Entity = log.Entity,
                    EntityId = log.EntityId,
                    Description = log.Description,
                    CreatedAt = log.CreatedAt
                };

                responseList.Add(dto);
            }

            return Ok(responseList);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}/GetAuditLogsByUser")]
        public async Task<IActionResult> GetAuditLogsByUser(int userId)
        {

            var logs = await _auditLogService.GetByUserIdAsync(userId);

            if (logs == null || !logs.Any())
                return NotFound("No audit logs found for this user");

            var responseList = logs.Select(log => new AuditLogResponseDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.User != null ? log.User.Name : null,
                UserEmail = log.User != null ? log.User.Email : null,
                Action = log.Action,
                Entity = log.Entity,
                EntityId = log.EntityId,
                Description = log.Description,
                CreatedAt = log.CreatedAt
            }).ToList();

            return Ok(responseList);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/GetAuditLog")]
        public async Task<IActionResult> GetAuditLog(int id)
        {
            var log = await _auditLogService.GetByIdAsync(id);

            if (log == null)
                return NotFound("Audit log not found");

            var response = new AuditLogResponseDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.User != null ? log.User.Name : null,
                UserEmail = log.User != null ? log.User.Email : null,
                Action = log.Action,
                Entity = log.Entity,
                EntityId = log.EntityId,
                Description = log.Description,
                CreatedAt = log.CreatedAt
            };

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAuditLogsByEntity")]
        public async Task<IActionResult> GetAuditLogsByEntity([FromQuery] string entity, [FromQuery] int? entityId)
        {

            var logs = await _auditLogService.GetByEntityAsync(entity, entityId);

            if (logs == null || !logs.Any())
                return NotFound("No audit logs found for the specified entity");

            var response = logs.Select(a => new AuditLogResponseDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.Name : null,
                UserEmail = a.User != null ? a.User.Email : null,
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                Description = a.Description,
                CreatedAt = a.CreatedAt
            }).ToList();

            return Ok(response);
        }

    }
}
