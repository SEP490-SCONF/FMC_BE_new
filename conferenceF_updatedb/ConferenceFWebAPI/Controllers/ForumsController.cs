using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Forums;
using AutoMapper;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForumsController : ControllerBase
    {
        private readonly IForumRepository _forumRepository;
        private readonly IForumQuestionRepository _forumQuestionRepository;
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IMapper _mapper;

        public ForumsController(
            IForumRepository forumRepository,
            IForumQuestionRepository forumQuestionRepository,
            IConferenceRepository conferenceRepository,
            IMapper mapper)
        {
            _forumRepository = forumRepository;
            _forumQuestionRepository = forumQuestionRepository;
            _conferenceRepository = conferenceRepository;
            _mapper = mapper;
        }

        // GET: api/Forums
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ForumDto>>> GetForums()
        {
            try
            {
                var forums = await _forumRepository.GetAll();
                var forumDtos = new List<ForumDto>();

                foreach (var forum in forums)
                {
                    // Get conference info
                    var conference = await _conferenceRepository.GetById(forum.ConferenceId);
                    
                    // Get questions count
                    var questions = await _forumQuestionRepository.GetByForumId(forum.ForumId);
                    
                    forumDtos.Add(new ForumDto
                    {
                        ForumId = forum.ForumId,
                        ConferenceId = forum.ConferenceId,
                        Title = forum.Title,
                        CreatedAt = forum.CreatedAt,
                        ConferenceTitle = conference?.Title ?? "Unknown Conference",
                        TotalQuestions = questions.Count()
                    });
                }

                return Ok(forumDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Forums/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ForumDto>> GetForum(int id)
        {
            try
            {
                var forum = await _forumRepository.GetById(id);
                if (forum == null)
                {
                    return NotFound($"Forum with ID {id} not found.");
                }

                // Get conference info
                var conference = await _conferenceRepository.GetById(forum.ConferenceId);
                
                // Get questions count
                var questions = await _forumQuestionRepository.GetByForumId(forum.ForumId);

                var forumDto = new ForumDto
                {
                    ForumId = forum.ForumId,
                    ConferenceId = forum.ConferenceId,
                    Title = forum.Title,
                    CreatedAt = forum.CreatedAt,
                    ConferenceTitle = conference?.Title ?? "Unknown Conference",
                    TotalQuestions = questions.Count()
                };

                return Ok(forumDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Forums/conference/5
        [HttpGet("conference/{conferenceId}")]
        public async Task<ActionResult<ForumDto>> GetForumByConferenceId(int conferenceId)
        {
            try
            {
                var forum = await _forumRepository.GetByConferenceId(conferenceId);
                if (forum == null)
                {
                    return NotFound($"No forum found for conference ID {conferenceId}.");
                }

                // Get conference info
                var conference = await _conferenceRepository.GetById(forum.ConferenceId);
                
                // Get questions count
                var questions = await _forumQuestionRepository.GetByForumId(forum.ForumId);

                var forumDto = new ForumDto
                {
                    ForumId = forum.ForumId,
                    ConferenceId = forum.ConferenceId,
                    Title = forum.Title,
                    CreatedAt = forum.CreatedAt,
                    ConferenceTitle = conference?.Title ?? "Unknown Conference",
                    TotalQuestions = questions.Count()
                };

                return Ok(forumDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Forums
        [HttpPost]
        public async Task<ActionResult<ForumDto>> CreateForum([FromBody] ForumCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify conference exists
                var conference = await _conferenceRepository.GetById(createDto.ConferenceId);
                if (conference == null)
                {
                    return BadRequest($"Conference with ID {createDto.ConferenceId} not found.");
                }

                // Check if forum already exists for this conference
                var existingForum = await _forumRepository.GetByConferenceId(createDto.ConferenceId);
                if (existingForum != null)
                {
                    return BadRequest($"Forum already exists for conference ID {createDto.ConferenceId}.");
                }

                var forum = new Forum
                {
                    ConferenceId = createDto.ConferenceId,
                    Title = createDto.Title,
                    CreatedAt = DateTime.UtcNow
                };

                await _forumRepository.Add(forum);

                var forumDto = new ForumDto
                {
                    ForumId = forum.ForumId,
                    ConferenceId = forum.ConferenceId,
                    Title = forum.Title,
                    CreatedAt = forum.CreatedAt,
                    ConferenceTitle = conference.Title,
                    TotalQuestions = 0
                };

                return CreatedAtAction(nameof(GetForum), new { id = forum.ForumId }, forumDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Forums/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForum(int id, [FromBody] ForumUpdateDto updateDto)
        {
            try
            {
                if (id != updateDto.ForumId)
                {
                    return BadRequest("Forum ID mismatch.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingForum = await _forumRepository.GetById(id);
                if (existingForum == null)
                {
                    return NotFound($"Forum with ID {id} not found.");
                }

                existingForum.Title = updateDto.Title;

                await _forumRepository.Update(existingForum);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Forums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForum(int id)
        {
            try
            {
                var forum = await _forumRepository.GetById(id);
                if (forum == null)
                {
                    return NotFound($"Forum with ID {id} not found.");
                }

                // Check if forum has questions
                var questions = await _forumQuestionRepository.GetByForumId(id);
                if (questions.Any())
                {
                    return BadRequest($"Cannot delete forum with existing questions. Please delete all questions first.");
                }

                await _forumRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Forums/5/questions-summary
        [HttpGet("{id}/questions-summary")]
        public async Task<ActionResult<object>> GetForumQuestionsSummary(int id)
        {
            try
            {
                var forum = await _forumRepository.GetById(id);
                if (forum == null)
                {
                    return NotFound($"Forum with ID {id} not found.");
                }

                var questions = await _forumQuestionRepository.GetByForumId(id);
                
                var summary = new
                {
                    ForumId = forum.ForumId,
                    ForumTitle = forum.Title,
                    TotalQuestions = questions.Count(),
                    RecentQuestions = questions
                        .OrderByDescending(q => q.CreatedAt)
                        .Take(5)
                        .Select(q => new
                        {
                            q.FqId,
                            q.Title,
                            q.CreatedAt,
                            AskedBy = q.AskBy // You might want to get user name from User table
                        })
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Forums/5/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateForum(int id)
        {
            try
            {
                var forum = await _forumRepository.GetById(id);
                if (forum == null)
                {
                    return NotFound($"Forum with ID {id} not found.");
                }

                // You can add activation logic here if needed
                // For now, just return success
                return Ok(new { message = $"Forum '{forum.Title}' is now active." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Forums/search?title=keyword
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ForumDto>>> SearchForums([FromQuery] string? title, [FromQuery] int? conferenceId)
        {
            try
            {
                var forums = await _forumRepository.GetAll();
                
                if (!string.IsNullOrEmpty(title))
                {
                    forums = forums.Where(f => f.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
                }
                
                if (conferenceId.HasValue)
                {
                    forums = forums.Where(f => f.ConferenceId == conferenceId.Value);
                }

                var forumDtos = new List<ForumDto>();

                foreach (var forum in forums)
                {
                    // Get conference info
                    var conference = await _conferenceRepository.GetById(forum.ConferenceId);
                    
                    // Get questions count
                    var questions = await _forumQuestionRepository.GetByForumId(forum.ForumId);
                    
                    forumDtos.Add(new ForumDto
                    {
                        ForumId = forum.ForumId,
                        ConferenceId = forum.ConferenceId,
                        Title = forum.Title,
                        CreatedAt = forum.CreatedAt,
                        ConferenceTitle = conference?.Title ?? "Unknown Conference",
                        TotalQuestions = questions.Count()
                    });
                }

                return Ok(forumDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
