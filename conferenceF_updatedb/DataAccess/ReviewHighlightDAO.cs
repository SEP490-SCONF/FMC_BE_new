using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ReviewHighlightDAO
    {
        private readonly ConferenceFTestContext _context;

        public ReviewHighlightDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewHighlight>> GetAll()
        {
            try
            {
                return await _context.ReviewHighlights.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving review highlights.", ex);
            }
        }

        public async Task<ReviewHighlight> GetById(int id)
        {
            try
            {
                return await _context.ReviewHighlights
                    .AsNoTracking()
                    .FirstOrDefaultAsync(rh => rh.HighlightId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving review highlight with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<ReviewHighlight>> GetByReviewId(int reviewId)
        {
            try
            {
                return await _context.ReviewHighlights
                    .Where(rh => rh.ReviewId == reviewId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving highlights for review ID {reviewId}.", ex);
            }
        }

        public async Task Add(ReviewHighlight highlight)
        {
            try
            {
                _context.ReviewHighlights.Add(highlight);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding review highlight.", ex);
            }
        }

        public async Task Update(ReviewHighlight highlight)
        {
            try
            {
                var existing = await _context.ReviewHighlights.FindAsync(highlight.HighlightId);
                if (existing == null)
                    throw new Exception($"ReviewHighlight with ID {highlight.HighlightId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(highlight);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating review highlight with ID {highlight.HighlightId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var highlight = await _context.ReviewHighlights.FindAsync(id);
                if (highlight != null)
                {
                    _context.ReviewHighlights.Remove(highlight);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Review highlight with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting review highlight with ID {id}.", ex);
            }
        }
    }

}
