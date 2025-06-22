using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class AnswerLikeRepository : IAnswerLikeRepository
    {
        private readonly AnswerLikeDAO _dao;

        public AnswerLikeRepository(AnswerLikeDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<AnswerLike>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<AnswerLike> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(AnswerLike entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(AnswerLike entity)
        {
            throw new System.NotImplementedException("Update is not implemented for AnswerLike.");
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<AnswerLike>> GetByAnswerId(int answerId)
        {
            return await _dao.GetByAnswerId(answerId);
        }
    }
}
