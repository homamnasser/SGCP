using SGCP.DTOs.Requests;
using SGCP.Models;

namespace SGCP.IService
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(User user);

    }
}
