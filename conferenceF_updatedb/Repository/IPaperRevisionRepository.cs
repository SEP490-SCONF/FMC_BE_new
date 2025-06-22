using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaperRevisionRepository : IRepositoryBase<PaperRevision>
    {
        Task<IEnumerable<PaperRevision>> GetByPaperId(int paperId);
    }
}
