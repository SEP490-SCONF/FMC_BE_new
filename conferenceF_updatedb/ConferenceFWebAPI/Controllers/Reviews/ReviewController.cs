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



        public ReviewController(IReviewRepository reviewRepository, IMapper mapper, IReviewCommentRepository commentRepository, IReviewHighlightRepository highlightRepository, IPaperRevisionRepository paperRevisionRepository,IPaperRepository paperRepository)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _commentRepository = commentRepository;
            _highlightRepository = highlightRepository;
            _paperRevisionRepository = paperRevisionRepository;
            _paperRepository = paperRepository;
        }

        // GET: api/Review
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _reviewRepository.GetAll();
            var result = _mapper.Map<IEnumerable<ReviewWithHighlightAndCommentDTO>>(reviews);
            return Ok(result);
        }

        // GET: api/Review/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var review = await _reviewRepository.GetById(id);
        //    if (review == null)
        //        return NotFound($"Review with ID {id} not found.");

        //    return Ok(_mapper.Map<ReviewWithHighlightAndCommentDTO>(review));
        //}

        //// GET: api/Review/paper/{paperId}
        //[HttpGet("paper/{paperId}")]
        //public async Task<IActionResult> GetByPaperId(int paperId)
        //{
        //    var reviews = await _reviewRepository.GetReviewsByPaperId(paperId);
        //    var result = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        //    return Ok(result);
        //}

        // POST: api/Review
        //[HttpPost]
        //public async Task<IActionResult> Add([FromForm] AddReviewDTO dto)
        //{
        //    var review = _mapper.Map<Review>(dto);

        //    review.Status = "Draft";
        //    review.ReviewedAt = DateTime.Now;

        //    await _reviewRepository.Add(review);

        //    var result = _mapper.Map<ReviewDTO>(review);
        //    return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, result);
        //}


        //// PUT: api/Review/{id}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(int id, [FromForm] UpdateReviewDTO dto)
        //{
        //    var review = await _reviewRepository.GetById(id);
        //    if (review == null)
        //        return NotFound($"Review with ID {id} not found.");

        //    _mapper.Map(dto, review);
        //    review.ReviewedAt = DateTime.Now;
        //    await _reviewRepository.Update(review);

        //    return NoContent();
        //}

        //// DELETE: api/Review/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var review = await _reviewRepository.GetById(id);
        //    if (review == null)
        //        return NotFound($"Review with ID {id} not found.");

        //    await _reviewRepository.Delete(id);
        //    return NoContent();
        //}

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

            // Gán thủ công để review có đầy đủ navigation (nếu lazy loading không bật)
            review.ReviewHighlights.Add(highlight);
            review.ReviewComments.Add(comment);
            review.Revision = revision;

            var result = _mapper.Map<ReviewWithHighlightAndCommentDTO>(review);
            return Ok(result);
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

        //[HttpPut("{reviewId}/update-status")]
        //public async Task<IActionResult> UpdateRevisionStatusAndForceCompleteReview(int reviewId, [FromForm] string revisionStatus)
        //{
        //    var review = await _reviewRepository.GetById(reviewId);
        //    if (review == null)
        //        return NotFound($"Review with ID {reviewId} not found.");

        //    var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(review.RevisionId);
        //    if (revision == null)
        //        return NotFound($"Revision with ID {review.RevisionId} not found.");

        //    // Update Revision
        //    revision.Status = revisionStatus;
        //    await _paperRevisionRepository.UpdatePaperRevisionAsync(revision);

        //    // Update Paper
        //    var paper = await _paperRepository.GetPaperByIdAsync(revision.PaperId);
        //    if (paper != null)
        //    {
        //        paper.Status = revisionStatus;
        //        await _paperRepository.UpdatePaperAsync(paper);
        //    }

        //    // Update Review
        //    review.Status = "Completed";
        //    await _reviewRepository.Update(review);

        //    return Ok(new
        //    {
        //        message = "Revision and Paper statuses updated. Review marked as Completed.",
        //        revisionStatus = revision.Status,
        //        paperStatus = paper?.Status,
        //        reviewStatus = review.Status
        //    });
        //}






        [HttpGet("WithHighlightAndComment/{reviewId}")]
        public async Task<IActionResult> GetDetailByReviewId(int reviewId)
        {
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
                return NotFound();

            var result = _mapper.Map<ReviewWithHighlightAndCommentDTO>(review);
            return Ok(result);
        }



    }
}
