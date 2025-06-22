using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class AnswerQuestionDAO
    {
        private readonly ConferenceFTestContext _context;

        public AnswerQuestionDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnswerQuestion>> GetAll()
        {
            try
            {
                return await _context.AnswerQuestions.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all answers.", ex);
            }
        }

        public async Task<AnswerQuestion> GetById(int id)
        {
            try
            {
                return await _context.AnswerQuestions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AnswerId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving answer with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<AnswerQuestion>> GetByQuestionId(int questionId)
        {
            try
            {
                return await _context.AnswerQuestions
                    .Where(a => a.FqId == questionId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving answers for question ID {questionId}.", ex);
            }
        }

        public async Task Add(AnswerQuestion answer)
        {
            try
            {
                _context.AnswerQuestions.Add(answer);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding answer.", ex);
            }
        }

        public async Task Update(AnswerQuestion answer)
        {
            try
            {
                var existing = await _context.AnswerQuestions.FindAsync(answer.AnswerId);
                if (existing == null)
                    throw new Exception($"Answer with ID {answer.AnswerId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(answer);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating answer with ID {answer.AnswerId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var answer = await _context.AnswerQuestions.FindAsync(id);
                if (answer != null)
                {
                    _context.AnswerQuestions.Remove(answer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Answer with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting answer with ID {id}.", ex);
            }
        }
    }

}
