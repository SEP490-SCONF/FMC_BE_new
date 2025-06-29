using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class ReviewDAO
    {
        private readonly ConferenceFTestContext _context;

        public ReviewDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all reviews (with Revision included)
        public async Task<IEnumerable<Review>> GetAllReviews()
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.Revision)
                    .Include(r => r.Paper) 

                    .Include(r => r.ReviewHighlights)
                    .Include(r => r.ReviewComments)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all reviews.", ex);
            }
        }


        public async Task<Review> GetReviewById(int id)
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.Revision)
                    .Include(r => r.Paper) 

                    .Include(r => r.ReviewHighlights)
                    .Include(r => r.ReviewComments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReviewId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving review with ID {id}.", ex);
            }
        }

        // Add a new review
        public async Task AddReview(Review review)
        {
            try
            {
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding new review.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding new review.", ex);
            }
        }

        // Update existing review
        public async Task UpdateReview(Review review)
        {
            try
            {
                var existing = await _context.Reviews.FindAsync(review.ReviewId);
                if (existing == null)
                    throw new Exception($"Review with ID {review.ReviewId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(review);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while updating the review.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating review with ID {review.ReviewId}.", ex);
            }
        }

        // Delete review by ID
        public async Task DeleteReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Review with ID {id} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting the review.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting review with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Review>> GetReviewsByPaperId(int paperId)
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.Revision) 
                    .Where(r => r.PaperId == paperId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving reviews for paper ID {paperId}.", ex);
            }
        }

        public async Task<int> GetReviewCount()
        {
            try
            {
                return await _context.Reviews.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while counting reviews.", ex);
            }
        }
    }
}
