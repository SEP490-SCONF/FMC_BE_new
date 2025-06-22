using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class AnswerQuestionRepository : IAnswerQuestionRepository
    {
        private readonly AnswerQuestionDAO _dao;

        public AnswerQuestionRepository(AnswerQuestionDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<AnswerQuestion>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<AnswerQuestion> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(AnswerQuestion entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(AnswerQuestion entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<AnswerQuestion>> GetByQuestionId(int questionId)
        {
            return await _dao.GetByQuestionId(questionId);
        }
    }
}
