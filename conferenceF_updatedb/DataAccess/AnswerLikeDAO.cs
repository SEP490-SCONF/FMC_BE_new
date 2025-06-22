using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class AnswerLikeDAO
    {
        private readonly ConferenceFTestContext _context;

        public AnswerLikeDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnswerLike>> GetAll()
        {
            try
            {
                return await _context.AnswerLikes.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all answer likes.", ex);
            }
        }

        public async Task<AnswerLike> GetById(int id)
        {
            try
            {
                return await _context.AnswerLikes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.LikeId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving answer like with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<AnswerLike>> GetByAnswerId(int answerId)
        {
            try
            {
                return await _context.AnswerLikes
                    .Where(a => a.AnswerId == answerId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving likes for answer ID {answerId}.", ex);
            }
        }

        public async Task Add(AnswerLike like)
        {
            try
            {
                _context.AnswerLikes.Add(like);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding answer like.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var like = await _context.AnswerLikes.FindAsync(id);
                if (like != null)
                {
                    _context.AnswerLikes.Remove(like);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Answer like with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting answer like with ID {id}.", ex);
            }
        }
    }

}
