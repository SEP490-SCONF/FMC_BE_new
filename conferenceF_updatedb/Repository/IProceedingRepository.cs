using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IProceedingRepository
    {
        Task<Proceeding> CreateProceedingAsync(Proceeding proceeding);
        Task<Proceeding?> GetProceedingByIdAsync(int proceedingId);
        Task<Proceeding?> GetProceedingByConferenceIdAsync(int conferenceId);
        Task UpdateProceedingAsync(Proceeding proceeding);
        Task<List<Paper>> GetPublishedPapersByConferenceAsync(int conferenceId);
    }
}
