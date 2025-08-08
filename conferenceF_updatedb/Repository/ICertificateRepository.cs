using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface ICertificateRepository : IRepositoryBase<Certificate>
    {
        Task<Certificate> GetByRegistrationId(int regId);
        Task<IEnumerable<Certificate>> GetByUserId(int userId);
        Task<IEnumerable<Certificate>> GetByConferenceId(int conferenceId);
        Task<bool> ExistsByRegistrationId(int regId);
        Task<bool> IsPaymentCompleted(int regId);
        Task<Certificate> GenerateCertificate(int regId);
        Task<Certificate> GenerateCertificateForApprovedPaper(int paperId, int authorId);
        Task<byte[]> GenerateCertificatePdf(int certificateId);
        Task<IEnumerable<Certificate>> GetCertificatesByPaperId(int paperId);

    }
}
