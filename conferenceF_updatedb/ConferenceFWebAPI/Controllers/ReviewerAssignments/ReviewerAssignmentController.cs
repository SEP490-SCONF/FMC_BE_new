using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.ReviewerAssignments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.ReviewerAssignments
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewerAssignmentController : ControllerBase
    {
        private readonly IReviewerAssignmentRepository _repository;
        private readonly IUserConferenceRoleRepository _userConferenceRoleRepository;
        private readonly IPaperRepository _paperRepository;

        private readonly IMapper _mapper;

        public ReviewerAssignmentController(IReviewerAssignmentRepository repository, IMapper mapper, IUserConferenceRoleRepository userConferenceRoleRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _userConferenceRoleRepository = userConferenceRoleRepository;
        }

        // GET: api/ReviewerAssignment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assignments = await _repository.GetAll();
            var result = _mapper.Map<IEnumerable<ReviewerAssignmentDTO>>(assignments);
            return Ok(result);
        }

        // GET: api/ReviewerAssignment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assignment = await _repository.GetById(id);
            if (assignment == null)
                return NotFound($"Assignment with ID {id} not found.");

            return Ok(_mapper.Map<ReviewerAssignmentDTO>(assignment));
        }

        // GET: api/ReviewerAssignment/paper/{paperId}
        [HttpGet("paper/{paperId}")]
        public async Task<IActionResult> GetByPaperId(int paperId)
        {
            var assignments = await _repository.GetByPaperId(paperId);
            var result = _mapper.Map<IEnumerable<ReviewerAssignmentDTO>>(assignments);
            return Ok(result);
        }

        //POST: api/ReviewerAssignment
       [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddReviewerAssignmentDTO dto)
        {
            var isReviewer = await _userConferenceRoleRepository.IsReviewer(dto.ReviewerId);
            if (!isReviewer)
            {
                return BadRequest($"User with ID {dto.ReviewerId} is not a Reviewer (ConferenceRoleId = 1).");
            }

            var entity = _mapper.Map<ReviewerAssignment>(dto);
            await _repository.Add(entity);

            var result = _mapper.Map<ReviewerAssignmentDTO>(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.AssignmentId }, result);
        }


        // PUT: api/ReviewerAssignment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewerAssignmentDTO dto)
        {
            var existing = await _repository.GetById(id);
            if (existing == null)
                return NotFound($"Assignment with ID {id} not found.");

            // ✅ Chỉ cập nhật nếu được truyền vào
            if (dto.PaperId.HasValue)
                existing.PaperId = dto.PaperId.Value;

            if (dto.ReviewerId.HasValue)
                existing.ReviewerId = dto.ReviewerId.Value;

            await _repository.Update(existing);
            return NoContent();
        }


        // DELETE: api/ReviewerAssignment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetById(id);
            if (existing == null)
                return NotFound($"Assignment with ID {id} not found.");

            await _repository.Delete(id);
            return NoContent();
        }

        // GET: api/ReviewerAssignment/reviewer/{reviewerId}
        [HttpGet("reviewer/{reviewerId}")]
        public async Task<IActionResult> GetByReviewerId(int reviewerId)
        {
            var assignments = await _repository.GetByReviewerId(reviewerId);
            var result = _mapper.Map<IEnumerable<ReviewerAssignmentDTO>>(assignments);
            return Ok(result);
        }

    }
}

