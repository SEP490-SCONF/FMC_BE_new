using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class PaperRevisionRepository : IPaperRevisionRepository
    {
        private readonly PaperRevisionDAO _revisionDao;

        public PaperRevisionRepository(PaperRevisionDAO revisionDao)
        {
            _revisionDao = revisionDao;
        }

        public async Task<IEnumerable<PaperRevision>> GetAll()
        {
            return await _revisionDao.GetAll();
        }

        public async Task<PaperRevision> GetById(int id)
        {
            return await _revisionDao.GetById(id);
        }

        public async Task Add(PaperRevision entity)
        {
            await _revisionDao.Add(entity);
        }

        public async Task Update(PaperRevision entity)
        {
            await _revisionDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _revisionDao.Delete(id);
        }

        public async Task<IEnumerable<PaperRevision>> GetByPaperId(int paperId)
        {
            return await _revisionDao.GetByPaperId(paperId);
        }
    }
}
