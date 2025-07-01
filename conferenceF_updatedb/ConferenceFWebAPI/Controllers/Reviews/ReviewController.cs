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
        private readonly IPaperRepository _paperRepository;

        public ReviewController(
            IReviewRepository reviewRepository,
            IMapper mapper,
            IReviewCommentRepository commentRepository,
            IReviewHighlightRepository highlightRepository,
            IPaperRevisionRepository paperRevisionRepository,
            IPaperRepository paperRepository)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _commentRepository = commentRepository;
            _highlightRepository = highlightRepository;
            _paperRevisionRepository = paperRevisionRepository;
            _paperRepository = paperRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _reviewRepository.GetAll();
            var result = _mapper.Map<IEnumerable<ReviewWithHighlightAndCommentDTO>>(reviews);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] AddReviewDTO dto)
        {
            var existingReview = await _reviewRepository.GetByRevisionId(dto.RevisionId);
            if (existingReview != null)
            {
                existingReview.Status = "Draft";
                existingReview.ReviewedAt = DateTime.Now;
                await _reviewRepository.Update(existingReview);
                var result = _mapper.Map<ReviewDTO>(existingReview);
                return Ok(result);
            }

            var review = _mapper.Map<Review>(dto);
            review.Status = "Draft";
            review.ReviewedAt = DateTime.Now;
            await _reviewRepository.Add(review);

            var resultNew = _mapper.Map<ReviewDTO>(review);
            return CreatedAtAction(nameof(GetAll), new { id = review.ReviewId }, resultNew);
        }

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

        [HttpPost("WithHighlightAndComment")]
        public async Task<IActionResult> AddWithHighlightAndComment([FromForm] AddReviewWithHighlightAndCommentDTO dto)
        {
            var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(dto.RevisionId);
            if (revision == null)
                return BadRequest("Invalid revision");

            var review = _mapper.Map<Review>(dto);
            review.PaperId = revision.PaperId;
            review.Status = "Draft";
            review.ReviewedAt = DateTime.Now;
            await _reviewRepository.Add(review);

            var savedReview = await _reviewRepository.GetByRevisionId(dto.RevisionId);
            if (savedReview == null)
                return NotFound("Saved review not found.");

            var highlight = new ReviewHighlight
            {
                ReviewId = savedReview.ReviewId,
                PageNumber = dto.PageNumber,
                OffsetStart = dto.OffsetStart,
                OffsetEnd = dto.OffsetEnd,
                TextHighlighted = dto.TextHighlighted,
                CreatedAt = DateTime.Now
            };
            await _highlightRepository.Add(highlight);

            var comment = new ReviewComment
            {
                ReviewId = savedReview.ReviewId,
                HighlightId = highlight.HighlightId,
                UserId = dto.UserId,
                CommentText = dto.CommentText,
                Status = dto.Status,
                CreatedAt = DateTime.Now
            };
            await _commentRepository.Add(comment);

            savedReview.ReviewHighlights = new List<ReviewHighlight> { highlight };
            savedReview.ReviewComments = new List<ReviewComment> { comment };
            savedReview.Revision = revision;

            var result = _mapper.Map<ReviewWithHighlightAndCommentDTO>(savedReview);
            return Ok(result);
        }

        [HttpPut("WithHighlightAndComment/{reviewId}")]
        public async Task<IActionResult> UpdateWithHighlightAndComment(int reviewId, [FromForm] UpdateReviewWithHighlightAndCommentDTO dto)
        {
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
                return NotFound($"Review with ID {reviewId} not found.");

            var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(dto.RevisionId);
            if (revision == null)
                return BadRequest($"Revision with ID {dto.RevisionId} does not exist.");

            _mapper.Map(dto, review);
            review.PaperId = revision.PaperId;
            review.ReviewedAt = DateTime.Now;
            review.Status = "Draft";
            await _reviewRepository.Update(review);

            var highlight = await _highlightRepository.GetById(dto.HighlightId);
            if (highlight == null)
                return NotFound($"Highlight with ID {dto.HighlightId} not found.");
            _mapper.Map(dto, highlight);
            await _highlightRepository.Update(highlight);

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

            var highlights = await _highlightRepository.GetByReviewId(reviewId);
            var comments = await _commentRepository.GetByReviewId(reviewId);

            var resultDto = new ReviewWithHighlightAndCommentDTO
            {
                ReviewId = review.ReviewId,
                PaperId = review.PaperId,
                ReviewerId = review.ReviewerId,
                RevisionId = review.RevisionId,
                Score = review.Score,
                Comment = review.Comments,
                Status = review.Status,
                ReviewedAt = review.ReviewedAt,
                Highlights = highlights.Select(h => new HighlightDTO
                {
                    HighlightId = h.HighlightId,
                    PageNumber = h.PageNumber,
                    OffsetStart = h.OffsetStart,
                    OffsetEnd = h.OffsetEnd,
                    TextHighlighted = h.TextHighlighted
                }).ToList(),
                Comments = comments.Select(c => new CommentsDTO
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    CommentText = c.CommentText,
                    CommentStatus = c.Status,
                    CreatedAt = c.CreatedAt,
                    HighlightId = c.HighlightId ?? 0
                }).ToList()
            };

            return Ok(resultDto);
        }

        [HttpGet("assignment/{assignmentId}")]
        public async Task<IActionResult> GetReviewByAssignmentId(int assignmentId)
        {
            var review = await _reviewRepository.GetReviewByAssignmentId(assignmentId);
            if (review == null)
                return NotFound($"Review not found for Assignment ID {assignmentId}");

            var assignment = await _reviewRepository.GetReviewByAssignmentId(assignmentId);
            var revisionStatus = assignment?.Paper?.PaperRevisions?.FirstOrDefault(r => r.Status == "Under Review")?.Status;

            var result = _mapper.Map<ReviewDTO>(review);
            result.PaperRevisionStatus = revisionStatus;

            return Ok(result);
        }

        [HttpPost("SendFeedback")]
        public async Task<IActionResult> SendFeedback([FromForm] int reviewId)
        {
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
                return NotFound($"Review with ID {reviewId} not found.");

            string paperStatus = review.PaperStatus;
            await _reviewRepository.UpdatePaperAndRevisionStatus(review.PaperId, paperStatus, review.RevisionId);

            return Ok("Feedback sent and statuses updated.");
        }
    }
}
