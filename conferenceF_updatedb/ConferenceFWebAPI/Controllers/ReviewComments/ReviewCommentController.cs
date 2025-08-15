using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.ReviewComments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.ReviewComments
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewCommentController : ControllerBase
    {
        private readonly IReviewCommentRepository _repository;
        private readonly IMapper _mapper;

        public ReviewCommentController(IReviewCommentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET: api/ReviewComment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewCommentDTO>>> GetAll()
        {
            var comments = await _repository.GetAll();
            return Ok(_mapper.Map<IEnumerable<ReviewCommentDTO>>(comments));
        }

        // GET: api/ReviewComment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewCommentDTO>> GetById(int id)
        {
            var comment = await _repository.GetById(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ReviewCommentDTO>(comment));
        }

        // GET: api/ReviewComment/ByReview/5
        [HttpGet("ByReview/{reviewId}")]
        public async Task<ActionResult<IEnumerable<ReviewCommentDTO>>> GetByReviewId(int reviewId)
        {
            var comments = await _repository.GetByReviewId(reviewId);
            return Ok(_mapper.Map<IEnumerable<ReviewCommentDTO>>(comments));
        }

        // POST: api/ReviewComment
        [HttpPost]
        public async Task<ActionResult> Add([FromForm] AddReviewCommentDTO dto)
        {
            var comment = _mapper.Map<ReviewComment>(dto);
            comment.CreatedAt = DateTime.Now;

            await _repository.Add(comment);
            return CreatedAtAction(nameof(GetById), new { id = comment.CommentId }, comment);
        }

        // PUT: api/ReviewComment/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromForm] UpdateReviewCommentDTO dto)
        {
            var existing = await _repository.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, existing);
            existing.CommentId = id;

            await _repository.Update(existing);
            return NoContent();
        }

        // DELETE: api/ReviewComment/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _repository.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _repository.Delete(id);
            return NoContent();
        }
    }
}
