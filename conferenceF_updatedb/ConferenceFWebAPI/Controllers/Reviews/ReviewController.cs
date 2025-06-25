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

        public ReviewController(IReviewRepository reviewRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
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
            await _reviewRepository.Add(review);

            var result = _mapper.Map<ReviewDTO>(review);
            return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, result);
        }

        // PUT: api/Review/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDTO dto)
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
    }
}
