using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Reviews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.Reviews
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly IReviewCommentRepository _commentRepository;
        private readonly IReviewHighlightRepository _highlightRepository;
        private readonly IPaperRevisionRepository _paperRevisionRepository;


        public ReviewController(IReviewRepository reviewRepository, IMapper mapper, IReviewCommentRepository commentRepository, IReviewHighlightRepository highlightRepository, IPaperRevisionRepository paperRevisionRepository)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _commentRepository = commentRepository;
            _highlightRepository = highlightRepository;
            _paperRevisionRepository = paperRevisionRepository;
        }

        // GET: api/Review
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _reviewRepository.GetAll();
            var result = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
            return Ok(result);
        }

        // GET: api/Review/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await _reviewRepository.GetById(id);
            if (review == null)
                return NotFound($"Review with ID {id} not found.");

            return Ok(_mapper.Map<ReviewDTO>(review));
        }

        // GET: api/Review/paper/{paperId}
        [HttpGet("paper/{paperId}")]
        public async Task<IActionResult> GetByPaperId(int paperId)
        {
            var reviews = await _reviewRepository.GetReviewsByPaperId(paperId);
            var result = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
            return Ok(result);
        }

        // POST: api/Review
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] AddReviewDTO dto)
        {
            // Kiểm tra xem review đã tồn tại cho PaperRevision này chưa
            var existingReview = await _reviewRepository.GetByRevisionId(dto.RevisionId);

            if (existingReview != null)
            {
                // Nếu review đã tồn tại, cập nhật review
                existingReview.Status = "Draft";  // Ví dụ cập nhật trạng thái
                existingReview.ReviewedAt = DateTime.Now;  // Cập nhật thời gian đánh giá

                await _reviewRepository.Update(existingReview);

                var result = _mapper.Map<ReviewDTO>(existingReview);
                return Ok(result);  // Trả về review đã cập nhật
            }

            // Nếu review chưa tồn tại, tạo mới review
            var review = _mapper.Map<Review>(dto);
            review.Status = "Draft";  // Trạng thái ban đầu là "Draft"
            review.ReviewedAt = DateTime.Now;

            await _reviewRepository.Add(review);

            var resultNew = _mapper.Map<ReviewDTO>(review);
            return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, resultNew);
        }



        // PUT: api/Review/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateReviewDTO dto)
        {
            var review = await _reviewRepository.GetById(id);
            if (review == null)
                return NotFound($"Review with ID {id} not found.");

            _mapper.Map(dto, review);
            review.ReviewedAt = DateTime.Now;
            await _reviewRepository.Update(review);

            return NoContent();
        }

        // DELETE: api/Review/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewRepository.GetById(id);
            if (review == null)
                return NotFound($"Review with ID {id} not found.");

            await _reviewRepository.Delete(id);
            return NoContent();
        }

        [HttpPost("WithHighlightAndComment")]
        public async Task<IActionResult> AddWithHighlightAndComment([FromForm] AddReviewWithHighlightAndCommentDTO dto)
        {
            // 1. Lấy PaperRevision để truy PaperId
            var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(dto.RevisionId);
            if (revision == null)
            {
                return BadRequest($"PaperRevision with ID {dto.RevisionId} does not exist.");
            }

            // 2. Tạo Review
            var review = await _reviewRepository.GetById(dto.ReviewId); // Đảm bảo gọi GetById để lấy review
            if (review == null)
            {
                return NotFound($"Review with ID {dto.ReviewId} not found.");
            }


            // 3. Tạo Highlight
            var highlight = new ReviewHighlight
            {
                ReviewId = review.ReviewId,
                PageIndex = dto.PageIndex, 
                Left = dto.Left,           
                Top = dto.Top,             
                Width = dto.Width,         
                Height = dto.Height,      
                TextHighlighted = dto.TextHighlighted,
                CreatedAt = DateTime.Now
            };
            await _highlightRepository.Add(highlight);

            // 4. Tạo Comment
            var comment = new ReviewComment
            {
                ReviewId = review.ReviewId,
                HighlightId = highlight.HighlightId,
                UserId = dto.UserId,
                CommentText = dto.CommentText,
                Status = dto.Status,
                CreatedAt = DateTime.Now
            };
            await _commentRepository.Add(comment);

            return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, new
            {
                Review = review,
                Highlight = highlight,
                Comment = comment
            });
        }
        [HttpPut("WithHighlightAndComment/{reviewId}")]
        public async Task<IActionResult> UpdateWithHighlightAndComment(int reviewId, [FromForm] UpdateReviewWithHighlightAndCommentDTO dto)
        {
            // 1. Kiểm tra Review có tồn tại không
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
                return NotFound($"Review with ID {reviewId} not found.");

            // 2. Lấy lại PaperId từ RevisionId
            var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(dto.RevisionId);
            if (revision == null)
                return BadRequest($"Revision with ID {dto.RevisionId} does not exist.");

            // 3. Cập nhật thông tin Review bằng AutoMapper
            _mapper.Map(dto, review);
            review.PaperId = revision.PaperId;
            review.ReviewedAt = DateTime.Now;
            review.Status = "Draft";  // Set trạng thái là Draft

            await _reviewRepository.Update(review);

            // 4. Cập nhật ReviewHighlight bằng AutoMapper
            var highlight = await _highlightRepository.GetById(dto.HighlightId);
            if (highlight == null)
                return NotFound($"Highlight with ID {dto.HighlightId} not found.");

            _mapper.Map(dto, highlight);  // Cập nhật highlight
            await _highlightRepository.Update(highlight);

            // 5. Lấy ReviewComment liên kết với ReviewHighlight
            var comment = await _commentRepository.GetByHighlightId(dto.HighlightId); // Lấy comment theo HighlightId
            if (comment == null)
            {
                // Nếu không tìm thấy comment, log thông báo lỗi
                return NotFound($"No comment found for HighlightId {dto.HighlightId}. Make sure the highlight has an associated comment.");
            }

            dto.CommentId = comment.CommentId;
            _mapper.Map(dto, comment);
            
            await _commentRepository.Update(comment);

            return NoContent();  // Trả về NoContent để chỉ ra rằng update thành công
        }


        [HttpGet("WithHighlightAndComment/{reviewId}")]
        public async Task<IActionResult> GetDetailByReviewId(int reviewId)
        {
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
                return NotFound($"Review with ID {reviewId} not found.");

            // Lấy tất cả highlights và comments cho reviewId
            var highlights = await _highlightRepository.GetByReviewId(reviewId);
            var comments = await _commentRepository.GetByReviewId(reviewId);

            // Tạo đối tượng DTO kết hợp review, highlights và comments
            var result = new ReviewWithHighlightAndCommentDTO
            {
                // Review
                ReviewId = review.ReviewId,
                PaperId = review.PaperId,
                ReviewerId = review.ReviewerId,
                RevisionId = review.RevisionId,
                Score = review.Score,
                Comment = review.Comments,
                Status = review.Status,
                ReviewedAt = review.ReviewedAt,

                // Highlight
                Highlights = highlights.Select(highlight => new HighlightDTO
                {
                    HighlightId = highlight.HighlightId,
                    PageIndex = highlight.PageIndex ?? 0,  
                    Left = highlight.Left ?? 0,            
                    Top = highlight.Top ?? 0,
                    Width = highlight.Width ?? 0,
                    Height = highlight.Height ?? 0,
                    TextHighlighted = highlight.TextHighlighted
                }).ToList(),

                // Comment
                Comments = comments.Select(comment => new CommentsDTO
                {
                    CommentId = comment.CommentId,
                    UserId = comment.UserId,
                    CommentText = comment.CommentText,
                    CommentStatus = comment.Status,
                    CreatedAt = comment.CreatedAt,
                    HighlightId = comment.HighlightId ?? 0
                }).ToList()
            };

            return Ok(result);
        }

        // GET: api/Review/assignment/{assignmentId}
        [HttpGet("assignment/{assignmentId}")]
        public async Task<IActionResult> GetReviewByAssignmentId(int assignmentId)
        {
            var review = await _reviewRepository.GetReviewByAssignmentId(assignmentId);

            if (review == null)
            {
                return NotFound($"Review not found for Assignment ID {assignmentId}");
            }

            // Lấy PaperRevision Status từ ReviewerAssignment
            var assignment = await _reviewRepository.GetReviewByAssignmentId(assignmentId);  // Tìm lại Assignment để lấy PaperRevision Status
            var revisionStatus = assignment?.Paper?.PaperRevisions?.FirstOrDefault(r => r.Status == "Under Review")?.Status;

            // Ánh xạ Review và PaperRevision Status vào DTO
            var result = _mapper.Map<ReviewDTO>(review);
            result.PaperRevisionStatus = revisionStatus;  // Thêm PaperRevisionStatus vào DTO

            return Ok(result);
        }
        // POST: api/Review/SendFeedback

        [HttpPost("SendFeedback")]
        public async Task<IActionResult> SendFeedback([FromForm] int reviewId)
        {
            // 1. Lấy Review từ database bằng ReviewId
            var review = await _reviewRepository.GetById(reviewId);

            // Nếu không tìm thấy review, trả về lỗi NotFound
            if (review == null)
            {
                return NotFound($"Review with ID {reviewId} not found.");
            }

            // 2. Lấy PaperStatus trực tiếp từ Review (không cần truyền nữa)
            string paperStatus = review.PaperStatus;

            // 3. Cập nhật Paper và PaperRevision status dựa trên PaperStatus
            await _reviewRepository.UpdatePaperAndRevisionStatus(review.PaperId, paperStatus, review.RevisionId);

            // 4. Trả về thông báo thành công
            return Ok("Feedback sent and statuses updated.");
        }




    }
}
