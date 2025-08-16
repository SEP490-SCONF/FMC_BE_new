using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.AnswerQuestions;
using AutoMapper;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerQuestionsController : ControllerBase
    {
        private readonly IAnswerQuestionRepository _answerQuestionRepository;
        private readonly IForumQuestionRepository _forumQuestionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ConferenceFTestContext _context;

        public AnswerQuestionsController(
            IAnswerQuestionRepository answerQuestionRepository,
            IForumQuestionRepository forumQuestionRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ConferenceFTestContext context)
        {
            _answerQuestionRepository = answerQuestionRepository;
            _forumQuestionRepository = forumQuestionRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
        }

        // GET: api/AnswerQuestions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerQuestionDto>>> GetAnswerQuestions()
        {
            try
            {
                var answers = await _answerQuestionRepository.GetAll();
                var answerDtos = new List<AnswerQuestionDto>();

                foreach (var answer in answers)
                {
                    var answerDto = await MapToAnswerDto(answer);
                    answerDtos.Add(answerDto);
                }

                return Ok(answerDtos.OrderByDescending(a => a.CreatedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnswerQuestionDto>> GetAnswerQuestion(int id)
        {
            try
            {
                var answer = await _answerQuestionRepository.GetById(id);
                if (answer == null)
                {
                    return NotFound($"Answer question with ID {id} not found.");
                }

                var answerDto = await MapToAnswerDto(answer);
                return Ok(answerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/question/{forumQuestionId}/paginated
        [HttpGet("question/{forumQuestionId}/paginated")]
        public async Task<ActionResult<PaginatedAnswerQuestionsDto>> GetAnswerQuestionsPaginated(
            int forumQuestionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            try
            {
                // Validate forum question exists
                var forumQuestion = await _context.ForumQuestions.FindAsync(forumQuestionId);
                if (forumQuestion == null)
                {
                    return NotFound($"Forum question with ID {forumQuestionId} not found.");
                }

                // Build query with search functionality
                var query = _context.AnswerQuestions
                    .Include(aq => aq.AnswerByNavigation)
                    .Include(aq => aq.ParentAnswer!)
                        .ThenInclude(pa => pa.AnswerByNavigation)
                    .Include(aq => aq.AnswerLikes)
                    .Include(aq => aq.InverseParentAnswer)
                    .Where(aq => aq.FqId == forumQuestionId);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(aq => aq.Answer.Contains(search));
                }

                // Get total count for pagination
                var totalItems = await query.CountAsync();

                // Calculate pagination values
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;

                // Get paginated results - Show parent answers first, then replies
                var answerQuestions = await query
                    .OrderBy(aq => aq.ParentAnswerId.HasValue ? 1 : 0) // Parent answers first
                    .ThenByDescending(aq => aq.CreatedAt) // Then by creation date
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                var answerDtos = answerQuestions.Select(aq => new AnswerQuestionDto
                {
                    AnswerId = aq.AnswerId,
                    FqId = aq.FqId,
                    AnswerBy = aq.AnswerBy,
                    ParentAnswerId = aq.ParentAnswerId,
                    Answer = aq.Answer,
                    CreatedAt = aq.CreatedAt,
                    AnswererName = aq.AnswerByNavigation?.Name ?? "Unknown",
                    AnswererEmail = aq.AnswerByNavigation?.Email,
                    ForumQuestionTitle = forumQuestion.Title,
                    TotalLikes = aq.AnswerLikes?.Count ?? 0,
                    HasReplies = aq.InverseParentAnswer?.Any() ?? false,
                    TotalReplies = aq.InverseParentAnswer?.Count ?? 0,
                    ParentAnswerText = aq.ParentAnswer?.Answer?.Length > 100 
                        ? aq.ParentAnswer.Answer.Substring(0, 100) + "..." 
                        : aq.ParentAnswer?.Answer,
                    ParentAnswererName = aq.ParentAnswer?.AnswerByNavigation?.Name
                }).ToList();

                var result = new PaginatedAnswerQuestionsDto
                {
                    Answers = answerDtos,
                    TotalCount = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages,
                    SearchTerm = search,
                    ForumQuestionId = forumQuestionId,
                    ForumQuestionTitle = forumQuestion.Title
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/question/{forumQuestionId}
        [HttpGet("question/{forumQuestionId}")]
        public async Task<ActionResult<IEnumerable<AnswerQuestionDto>>> GetAnswersByQuestionId(int forumQuestionId)
        {
            try
            {
                var answers = await _answerQuestionRepository.GetByQuestionId(forumQuestionId);
                var answerDtos = new List<AnswerQuestionDto>();

                foreach (var answer in answers)
                {
                    var answerDto = await MapToAnswerDto(answer);
                    answerDtos.Add(answerDto);
                }

                return Ok(answerDtos.OrderBy(a => a.ParentAnswerId.HasValue ? 1 : 0)
                                   .ThenByDescending(a => a.CreatedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AnswerQuestionDto>>> GetAnswersByUserId(int userId)
        {
            try
            {
                var allAnswers = await _answerQuestionRepository.GetAll();
                var userAnswers = allAnswers.Where(a => a.AnswerBy == userId);
                var answerDtos = new List<AnswerQuestionDto>();

                foreach (var answer in userAnswers)
                {
                    var answerDto = await MapToAnswerDto(answer);
                    answerDtos.Add(answerDto);
                }

                return Ok(answerDtos.OrderByDescending(a => a.CreatedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/AnswerQuestions
        [HttpPost]
        public async Task<ActionResult<AnswerQuestionDto>> CreateAnswerQuestion([FromBody] AnswerQuestionCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify forum question exists
                var forumQuestion = await _forumQuestionRepository.GetById(createDto.FqId);
                if (forumQuestion == null)
                {
                    return BadRequest($"Forum question with ID {createDto.FqId} not found.");
                }

                // Verify user exists
                var user = await _userRepository.GetById(createDto.AnswerBy);
                if (user == null)
                {
                    return BadRequest($"User with ID {createDto.AnswerBy} not found.");
                }

                // If this is a reply, verify parent answer exists and belongs to the same question
                if (createDto.ParentAnswerId.HasValue)
                {
                    var parentAnswer = await _answerQuestionRepository.GetById(createDto.ParentAnswerId.Value);
                    if (parentAnswer == null)
                    {
                        return BadRequest($"Parent answer with ID {createDto.ParentAnswerId} not found.");
                    }
                    if (parentAnswer.FqId != createDto.FqId)
                    {
                        return BadRequest("Parent answer must belong to the same forum question.");
                    }
                }

                var answerQuestion = new AnswerQuestion
                {
                    FqId = createDto.FqId,
                    AnswerBy = createDto.AnswerBy,
                    ParentAnswerId = createDto.ParentAnswerId,
                    Answer = createDto.Answer,
                    CreatedAt = DateTime.UtcNow
                };

                await _answerQuestionRepository.Add(answerQuestion);

                var answerDto = await MapToAnswerDto(answerQuestion);
                return CreatedAtAction(nameof(GetAnswerQuestion), new { id = answerQuestion.AnswerId }, answerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/AnswerQuestions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswerQuestion(int id, [FromBody] AnswerQuestionUpdateDto updateDto)
        {
            try
            {
                if (id != updateDto.AnswerId)
                {
                    return BadRequest("Answer ID mismatch.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingAnswer = await _answerQuestionRepository.GetById(id);
                if (existingAnswer == null)
                {
                    return NotFound($"Answer question with ID {id} not found.");
                }

                existingAnswer.Answer = updateDto.Answer;

                await _answerQuestionRepository.Update(existingAnswer);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/AnswerQuestions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswerQuestion(int id)
        {
            try
            {
                var answer = await _answerQuestionRepository.GetById(id);
                if (answer == null)
                {
                    return NotFound($"Answer question with ID {id} not found.");
                }

                // Check if this answer has replies
                var replies = await _answerQuestionRepository.GetAll();
                var hasReplies = replies.Any(r => r.ParentAnswerId == id);
                
                if (hasReplies)
                {
                    return BadRequest("Cannot delete answer that has replies. Delete the replies first.");
                }

                await _answerQuestionRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/search?content=keyword&forumQuestionId=1
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<AnswerQuestionSummaryDto>>> SearchAnswers(
            [FromQuery] string? content,
            [FromQuery] int? forumQuestionId,
            [FromQuery] int? userId)
        {
            try
            {
                var answers = await _answerQuestionRepository.GetAll();

                if (!string.IsNullOrEmpty(content))
                {
                    answers = answers.Where(a => a.Answer.Contains(content, StringComparison.OrdinalIgnoreCase));
                }

                if (forumQuestionId.HasValue)
                {
                    answers = answers.Where(a => a.FqId == forumQuestionId.Value);
                }

                if (userId.HasValue)
                {
                    answers = answers.Where(a => a.AnswerBy == userId.Value);
                }

                var answerSummaries = new List<AnswerQuestionSummaryDto>();

                foreach (var answer in answers.OrderByDescending(a => a.CreatedAt))
                {
                    var user = await _userRepository.GetById(answer.AnswerBy);
                    var allAnswers = await _answerQuestionRepository.GetAll();
                    var replies = allAnswers.Where(a => a.ParentAnswerId == answer.AnswerId);

                    answerSummaries.Add(new AnswerQuestionSummaryDto
                    {
                        AnswerId = answer.AnswerId,
                        Answer = answer.Answer.Length > 200 ? answer.Answer.Substring(0, 200) + "..." : answer.Answer,
                        AnswererName = user?.Name ?? "Unknown User",
                        CreatedAt = answer.CreatedAt,
                        TotalLikes = 0, // You can implement this based on your AnswerLike logic
                        TotalReplies = replies.Count(),
                        IsReply = answer.ParentAnswerId.HasValue
                    });
                }

                return Ok(answerSummaries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to map AnswerQuestion to AnswerQuestionDto
        private async Task<AnswerQuestionDto> MapToAnswerDto(AnswerQuestion answer)
        {
            var user = await _userRepository.GetById(answer.AnswerBy);
            var forumQuestion = await _forumQuestionRepository.GetById(answer.FqId);
            var allAnswers = await _answerQuestionRepository.GetAll();
            var replies = allAnswers.Where(a => a.ParentAnswerId == answer.AnswerId);
            
            AnswerQuestion? parentAnswer = null;
            User? parentAnswerer = null;
            if (answer.ParentAnswerId.HasValue)
            {
                parentAnswer = await _answerQuestionRepository.GetById(answer.ParentAnswerId.Value);
                if (parentAnswer != null)
                {
                    parentAnswerer = await _userRepository.GetById(parentAnswer.AnswerBy);
                }
            }

            return new AnswerQuestionDto
            {
                AnswerId = answer.AnswerId,
                FqId = answer.FqId,
                AnswerBy = answer.AnswerBy,
                ParentAnswerId = answer.ParentAnswerId,
                Answer = answer.Answer,
                CreatedAt = answer.CreatedAt,
                AnswererName = user?.Name ?? "Unknown User",
                AnswererEmail = user?.Email,
                ForumQuestionTitle = forumQuestion?.Title ?? "Unknown Question",
                TotalLikes = 0, // Implement based on your AnswerLike repository
                HasReplies = replies.Any(),
                TotalReplies = replies.Count(),
                ParentAnswerText = parentAnswer?.Answer?.Length > 100 
                    ? parentAnswer.Answer.Substring(0, 100) + "..." 
                    : parentAnswer?.Answer,
                ParentAnswererName = parentAnswerer?.Name
            };
        }
    }
}
