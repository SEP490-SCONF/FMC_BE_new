using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ForumQuestionDAO
    {
        private readonly ConferenceFTestContext _context;

        public ForumQuestionDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumQuestion>> GetAll()
        {
            try
            {
                return await _context.ForumQuestions.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving forum questions.", ex);
            }
        }

        public async Task<ForumQuestion> GetById(int id)
        {
            try
            {
                return await _context.ForumQuestions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FqId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving forum question with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<ForumQuestion>> GetByForumId(int forumId)
        {
            try
            {
                return await _context.ForumQuestions
                    .Where(f => f.ForumId == forumId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving questions for forum ID {forumId}.", ex);
            }
        }

        public async Task Add(ForumQuestion question)
        {
            try
            {
                _context.ForumQuestions.Add(question);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding forum question.", ex);
            }
        }

        public async Task Update(ForumQuestion question)
        {
            try
            {
                var existing = await _context.ForumQuestions.FindAsync(question.FqId);
                if (existing == null)
                    throw new Exception($"ForumQuestion with ID {question.FqId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(question);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating forum question with ID {question.FqId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var question = await _context.ForumQuestions.FindAsync(id);
                if (question != null)
                {
                    _context.ForumQuestions.Remove(question);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"ForumQuestion with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting forum question with ID {id}.", ex);
            }
        }
    }

}
