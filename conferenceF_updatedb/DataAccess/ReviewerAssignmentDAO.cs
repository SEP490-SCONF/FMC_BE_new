using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DataAccess
{
    public class ReviewerAssignmentDAO
    {
        private readonly ConferenceFTestContext _context;

        public ReviewerAssignmentDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetAll()
        {
            try
            {
                return await _context.ReviewerAssignments
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving reviewer assignments.", ex);
            }
        }

        public async Task<ReviewerAssignment> GetById(int id)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.AssignmentId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reviewer assignment with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetByPaperId(int paperId)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Where(r => r.PaperId == paperId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reviewer assignments for paper ID {paperId}.", ex);
            }
        }

        public async Task Add(ReviewerAssignment entity)
        {
            try
            {
                _context.ReviewerAssignments.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding reviewer assignment.", ex);
            }
        }

        public async Task Update(ReviewerAssignment entity)
        {
            try
            {
                var existing = await _context.ReviewerAssignments.FindAsync(entity.AssignmentId);
                if (existing == null)
                    throw new Exception($"ReviewerAssignment with ID {entity.AssignmentId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating reviewer assignment with ID {entity.AssignmentId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var entity = await _context.ReviewerAssignments.FindAsync(id);
                if (entity != null)
                {
                    _context.ReviewerAssignments.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"ReviewerAssignment with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting reviewer assignment with ID {id}.", ex);
            }
        }
    }
}
