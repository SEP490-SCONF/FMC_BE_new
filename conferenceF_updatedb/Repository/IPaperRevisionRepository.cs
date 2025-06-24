using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaperRevisionRepository
    {
        Task<PaperRevision?> GetPaperRevisionByIdAsync(int revisionId);
        Task<PaperRevision> AddPaperRevisionAsync(PaperRevision paperRevision);
        Task UpdatePaperRevisionAsync(PaperRevision paperRevision);
        Task DeletePaperRevisionAsync(int revisionId);
        Task<IEnumerable<PaperRevision>> GetRevisionsByPaperIdAsync(int paperId);
    }
}

