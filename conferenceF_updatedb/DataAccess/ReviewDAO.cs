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

        // Get all reviews (basic)
        public async Task<IEnumerable<Review>> GetAllReviews()
        {
            try
            {
                return await _context.Reviews
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all reviews.", ex);
            }
        }

        // Get review by ID
        public async Task<Review> GetReviewById(int id)
        {
            try
            {
                return await _context.Reviews
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

        // Get reviews by paper ID
        public async Task<IEnumerable<Review>> GetReviewsByPaperId(int paperId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.PaperId == paperId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving reviews for paper ID {paperId}.", ex);
            }
        }

        // Count reviews
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
        public async Task<Review> GetByRevisionId(int revisionId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.RevisionId == revisionId);  // Tìm Review theo RevisionId
        }
        public async Task<Review> GetReviewByAssignmentId(int assignmentId)
        {
            var assignment = await _context.ReviewerAssignments
                .Where(r => r.AssignmentId == assignmentId)
                .Include(r => r.Paper) // Bao gồm thông tin Paper
                .ThenInclude(p => p.PaperRevisions) // Bao gồm các phiên bản của bài báo
                .FirstOrDefaultAsync();

            if (assignment == null)
                return null;

            // Giả sử mỗi PaperRevision có một Review duy nhất
            var revision = assignment.Paper.PaperRevisions.FirstOrDefault(r => r.Status == "Under Review");

            if (revision == null)
                return null;

            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.RevisionId == revision.RevisionId); // Lấy Review dựa trên RevisionId
        }
        // Cập nhật trạng thái Paper và PaperRevision
        public async Task UpdatePaperAndRevisionStatus(int paperId, string paperStatus, int revisionId)
        {
            var paper = await _context.Papers
                .FirstOrDefaultAsync(p => p.PaperId == paperId);

            if (paper != null)
            {
                paper.Status = paperStatus;  // Cập nhật trạng thái của Paper

                var revision = await _context.PaperRevisions
                    .FirstOrDefaultAsync(r => r.PaperId == paperId && r.RevisionId == revisionId);

                if (revision != null)
                {
                    revision.Status = paperStatus;  // Cập nhật trạng thái của PaperRevision
                }

                await _context.SaveChangesAsync();  // Lưu thay đổi vào cơ sở dữ liệu
            }
        }

    }
}
