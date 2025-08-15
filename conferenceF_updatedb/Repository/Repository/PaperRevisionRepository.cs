using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class PaperRevisionRepository : IPaperRevisionRepository
    {
        private readonly PaperRevisionDAO _paperRevisionDAO; // <--- Inject lớp cụ thể PaperRevisionDAO

        public List<PaperRevision> GetPaperRevisionsByPaperId(int paperId)
        {
            return _paperRevisionDAO.GetPaperRevisionsByPaperId(paperId);
        }
        public PaperRevisionRepository(PaperRevisionDAO paperRevisionDAO) // <--- Constructor nhận lớp cụ thể
        {
            _paperRevisionDAO = paperRevisionDAO;
        }

        public async Task<PaperRevision?> GetPaperRevisionByIdAsync(int revisionId)
        {
            return await _paperRevisionDAO.GetPaperRevisionByIdAsync(revisionId);
        }

        public async Task<PaperRevision> AddPaperRevisionAsync(PaperRevision paperRevision)
        {
            return await _paperRevisionDAO.AddPaperRevisionAsync(paperRevision);
        }

        public async Task UpdatePaperRevisionAsync(PaperRevision paperRevision)
        {
            await _paperRevisionDAO.UpdatePaperRevisionAsync(paperRevision);
        }

        public async Task DeletePaperRevisionAsync(int revisionId)
        {
            await _paperRevisionDAO.DeletePaperRevisionAsync(revisionId);
        }

        public async Task<IEnumerable<PaperRevision>> GetRevisionsByPaperIdAsync(int paperId)
        {
            return await _paperRevisionDAO.GetRevisionsByPaperIdAsync(paperId);
        }

        public async Task<string?> GetAcceptedFilePathByPaperIdAsync(int paperId)
        {
            return await _paperRevisionDAO.GetAcceptedFilePathByPaperIdAsync(paperId);
        }

    }
}
