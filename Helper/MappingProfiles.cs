using AutoMapper;
using SGCP.DTOs.Responses;
using SGCP.Models;
using System.Diagnostics.Metrics;

namespace SGCP.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, AuthResponseDto>().ReverseMap();
            CreateMap<Role, RoleResponseDto>().ReverseMap();
            CreateMap<Government, GovernmentResponseDto>().ReverseMap();
            CreateMap<User, EmployeeResponseDto>().ReverseMap();
            CreateMap<ComplaintType, ComplaintTypeResponseDto>().ReverseMap();
            CreateMap<Complaint, ComplaintResponseDto>().ReverseMap();
            CreateMap<ComplaintAttachment, AttachmentResponseDto>().ReverseMap();





        }
    }

}