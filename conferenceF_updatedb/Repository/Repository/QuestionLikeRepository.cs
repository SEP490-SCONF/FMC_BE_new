using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class QuestionLikeRepository : IQuestionLikeRepository
    {
        private readonly QuestionLikeDAO _dao;

        public QuestionLikeRepository(QuestionLikeDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<QuestionLike>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<QuestionLike> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(QuestionLike entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(QuestionLike entity)
        {
            throw new System.NotImplementedException("Update is not implemented for QuestionLike.");
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<QuestionLike>> GetByQuestionId(int questionId)
        {
            return await _dao.GetByQuestionId(questionId);
        }
    }
}
