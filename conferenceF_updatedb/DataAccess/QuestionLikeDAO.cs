using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class QuestionLikeDAO
    {
        private readonly ConferenceFTestContext _context;

        public QuestionLikeDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuestionLike>> GetAll()
        {
            try
            {
                return await _context.QuestionLikes.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all question likes.", ex);
            }
        }

        public async Task<QuestionLike> GetById(int id)
        {
            try
            {
                return await _context.QuestionLikes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.LikeId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving question like with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<QuestionLike>> GetByQuestionId(int questionId)
        {
            try
            {
                return await _context.QuestionLikes
                    .Where(q => q.LikeId == questionId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving likes for question ID {questionId}.", ex);
            }
        }

        public async Task Add(QuestionLike like)
        {
            try
            {
                _context.QuestionLikes.Add(like);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding question like.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var like = await _context.QuestionLikes.FindAsync(id);
                if (like != null)
                {
                    _context.QuestionLikes.Remove(like);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Question like with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting question like with ID {id}.", ex);
            }
        }
    }

}
