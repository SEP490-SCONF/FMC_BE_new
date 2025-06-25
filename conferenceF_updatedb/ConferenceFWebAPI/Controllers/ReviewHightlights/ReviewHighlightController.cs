using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.ReviewHightlights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.ReviewHightlights
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewHighlightController : ControllerBase
    {
        private readonly IReviewHighlightRepository _repository;
        private readonly IMapper _mapper;

        public ReviewHighlightController(IReviewHighlightRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET: api/ReviewHighlight
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewHightlightDTO>>> GetAll()
        {
            var highlights = await _repository.GetAll();
            return Ok(_mapper.Map<IEnumerable<ReviewHightlightDTO>>(highlights));
        }

        // GET: api/ReviewHighlight/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewHightlightDTO>> GetById(int id)
        {
            var highlight = await _repository.GetById(id);
            if (highlight == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ReviewHightlightDTO>(highlight));
        }

        // GET: api/ReviewHighlight/ByReview/5
        [HttpGet("ByReview/{reviewId}")]
        public async Task<ActionResult<IEnumerable<ReviewHightlightDTO>>> GetByReviewId(int reviewId)
        {
            var highlights = await _repository.GetByReviewId(reviewId);
            return Ok(_mapper.Map<IEnumerable<ReviewHightlightDTO>>(highlights));
        }

        // POST: api/ReviewHighlight
        [HttpPost]
        public async Task<ActionResult> Add([FromForm] AddReviewHightlightDTO dto)
        {
            var highlight = _mapper.Map<ReviewHighlight>(dto);
            highlight.CreatedAt = DateTime.Now;

            await _repository.Add(highlight);
            return CreatedAtAction(nameof(GetById), new { id = highlight.HighlightId }, highlight);
        }

        // PUT: api/ReviewHighlight/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromForm] UpdateReviewHightlightDTO dto)
        {
            var existing = await _repository.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, existing);
            existing.HighlightId = id; 

            await _repository.Update(existing);
            return NoContent();
        }

        // DELETE: api/ReviewHighlight/5
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
