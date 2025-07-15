using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;


namespace DataAccess
{
    public class ReviewCommentDAO
    {
        private readonly ConferenceFTestContext _context;

        public ReviewCommentDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewComment>> GetAll()
        {
            try
            {
                return await _context.ReviewComments.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving review comments.", ex);
            }
        }

        public async Task<ReviewComment> GetById(int id)
        {
            try
            {
                return await _context.ReviewComments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(rc => rc.CommentId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving review comment with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<ReviewComment>> GetByReviewId(int reviewId)
        {
            try
            {
                return await _context.ReviewComments
                    .Where(rc => rc.ReviewId == reviewId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving comments for review ID {reviewId}.", ex);
            }
        }

        public async Task Add(ReviewComment comment)
        {
            try
            {
                _context.ReviewComments.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding review comment.", ex);
            }
        }

        public async Task Update(ReviewComment comment)
        {
            try
            {
                var existing = await _context.ReviewComments.FindAsync(comment.CommentId);
                if (existing == null)
                    throw new Exception($"Review comment with ID {comment.CommentId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating review comment with ID {comment.CommentId}.", ex);
            }
        }
        public async Task<ReviewComment> GetByHighlightId(int highlightId)
        {
            try
            {
                return await _context.ReviewComments
                    .Where(rc => rc.HighlightId == highlightId)  // Lọc theo HighlightId
                    .AsNoTracking()
                    .FirstOrDefaultAsync();  // Trả về comment đầu tiên, nếu có
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving comment for highlight ID {highlightId}.", ex);
            }
        }
        public async Task Delete(int id)
        {
            try
            {
                var comment = await _context.ReviewComments.FindAsync(id);
                if (comment != null)
                {
                    _context.ReviewComments.Remove(comment);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Review comment with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting review comment with ID {id}.", ex);
            }
        }
    }

}
