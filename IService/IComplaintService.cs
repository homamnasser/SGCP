using SGCP.Models;

namespace SGCP.IService
{
    public interface IComplaintService
    {
        ICollection<Complaint> GetComplaints();
        Task<Complaint?> GetComplaint(int id);
        Task<bool> ComplaintExists(int id);

        Task<bool> ComplaintExists(string refrunce);

        Task<Complaint> CreateComplaint(Complaint complaint);
        Task<bool> UpdateComplaint(Complaint complaint);
        Task<bool> DeleteType(Complaint complaint);
        Task<ICollection<Complaint>> GetComplaintsByGoverment(int govId);
        Task<ICollection<Complaint>> GetComplaintsByUser(int userId);

        Task<bool> Save();

        Task<bool> AddAttachment(ComplaintAttachment attachments);

        Task<ICollection<ComplaintAttachment>> GetAttachments(int complaintId);


    }
}
