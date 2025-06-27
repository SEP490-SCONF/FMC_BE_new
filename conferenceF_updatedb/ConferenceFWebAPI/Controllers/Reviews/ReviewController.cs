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
            var review = _mapper.Map<Review>(dto);

            review.Status = "Draft";
            review.ReviewedAt = DateTime.Now;

            await _reviewRepository.Add(review);

            var result = _mapper.Map<ReviewDTO>(review);
            return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, result);
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
            var review = _mapper.Map<Review>(dto);
            review.PaperId = revision.PaperId; // tự động gán từ revision
            review.Status = "Draft";
            review.ReviewedAt = DateTime.Now;
            await _reviewRepository.Add(review);

            // 3. Tạo Highlight
            var highlight = new ReviewHighlight
            {
                ReviewId = review.ReviewId,
                PageNumber = dto.PageNumber,
                OffsetStart = dto.OffsetStart,
                OffsetEnd = dto.OffsetEnd,
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
            review.Status = "Draft";

            await _reviewRepository.Update(review);

            // 4. Cập nhật ReviewHighlight bằng AutoMapper
            var highlight = await _highlightRepository.GetById(dto.HighlightId);
            if (highlight == null)
                return NotFound($"Highlight with ID {dto.HighlightId} not found.");

            _mapper.Map(dto, highlight);
            await _highlightRepository.Update(highlight);

            // 5. Cập nhật ReviewComment bằng AutoMapper
            var comment = await _commentRepository.GetById(dto.CommentId);
            if (comment == null)
                return NotFound($"Comment with ID {dto.CommentId} not found.");

            _mapper.Map(dto, comment);
            await _commentRepository.Update(comment);

            return NoContent();
        }
        [HttpGet("WithHighlightAndComment/{reviewId}")]
        public async Task<IActionResult> GetDetailByReviewId(int reviewId)
        {
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
                return NotFound($"Review with ID {reviewId} not found.");

            var highlight = (await _highlightRepository.GetByReviewId(reviewId)).FirstOrDefault();
            var comment = (await _commentRepository.GetByReviewId(reviewId)).FirstOrDefault();

            var result = new ReviewWithHighlightAndCommentDTO
            {
                // Review
                ReviewId = review.ReviewId,
                PaperId = review.PaperId,
                ReviewerId = review.ReviewerId,
                RevisionId = review.RevisionId,
                Score = review.Score,
                Comments = review.Comments,
                Status = review.Status,
                ReviewedAt = review.ReviewedAt,

                // Highlight
                HighlightId = highlight?.HighlightId ?? 0,
                PageNumber = highlight?.PageNumber,
                OffsetStart = highlight?.OffsetStart,
                OffsetEnd = highlight?.OffsetEnd,
                TextHighlighted = highlight?.TextHighlighted,

                // Comment
                CommentId = comment?.CommentId ?? 0,
                UserId = comment?.UserId ?? 0,
                CommentText = comment?.CommentText,
                CommentStatus = comment?.Status,
                CreatedAt = comment?.CreatedAt
            };

            return Ok(result);
        }


    }
}
