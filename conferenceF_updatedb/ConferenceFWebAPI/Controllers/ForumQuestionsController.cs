using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Forums;
using ConferenceFWebAPI.DTOs.AnswerQuestions;
using AutoMapper;
using Repository;
using System.Security.Claims;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForumQuestionsController : ControllerBase
    {
        private readonly IForumQuestionRepository _forumQuestionRepository;
        private readonly IForumRepository _forumRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAnswerQuestionRepository _answerQuestionRepository;
        private readonly IQuestionLikeRepository _questionLikeRepository;
        private readonly IMapper _mapper;
        private readonly ConferenceFTestContext _context;

        public ForumQuestionsController(
            IForumQuestionRepository forumQuestionRepository,
            IForumRepository forumRepository,
            IUserRepository userRepository,
            IAnswerQuestionRepository answerQuestionRepository,
            IQuestionLikeRepository questionLikeRepository,
            IMapper mapper,
            ConferenceFTestContext context)
        {
            _forumQuestionRepository = forumQuestionRepository;
            _forumRepository = forumRepository;
            _userRepository = userRepository;
            _answerQuestionRepository = answerQuestionRepository;
            _questionLikeRepository = questionLikeRepository;
            _mapper = mapper;
            _context = context;
        }

        // GET: api/ForumQuestions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ForumQuestionDto>>> GetForumQuestions()
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var questions = await _forumQuestionRepository.GetAll();
                var questionDtos = new List<ForumQuestionDto>();

                foreach (var question in questions)
                {
                    var questionDto = await MapToQuestionDto(question, currentUserId);
                    questionDtos.Add(questionDto);
                }

                return Ok(questionDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ForumQuestionDto>> GetForumQuestion(int id)
        {
            try
            {
                var question = await _forumQuestionRepository.GetById(id);
                if (question == null)
                {
                    return NotFound($"Forum question with ID {id} not found.");
                }

                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                var questionDto = await MapToQuestionDto(question, currentUserId);
                return Ok(questionDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/forum/5
        [HttpGet("forum/{forumId}")]
        public async Task<ActionResult<IEnumerable<ForumQuestionDto>>> GetQuestionsByForumId(int forumId)
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var questions = await _forumQuestionRepository.GetByForumId(forumId);
                var questionDtos = new List<ForumQuestionDto>();

                foreach (var question in questions)
                {
                    var questionDto = await MapToQuestionDto(question, currentUserId);
                    questionDtos.Add(questionDto);
                }

                return Ok(questionDtos.OrderByDescending(q => q.CreatedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ForumQuestionDto>>> GetQuestionsByUserId(int userId)
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var allQuestions = await _forumQuestionRepository.GetAll();
                var userQuestions = allQuestions.Where(q => q.AskBy == userId);
                var questionDtos = new List<ForumQuestionDto>();

                foreach (var question in userQuestions)
                {
                    var questionDto = await MapToQuestionDto(question, currentUserId);
                    questionDtos.Add(questionDto);
                }

                return Ok(questionDtos.OrderByDescending(q => q.CreatedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ForumQuestions
        [HttpPost]
        public async Task<ActionResult<ForumQuestionDto>> CreateForumQuestion([FromBody] ForumQuestionCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify forum exists
                var forum = await _forumRepository.GetById(createDto.ForumId);
                if (forum == null)
                {
                    return BadRequest($"Forum with ID {createDto.ForumId} not found.");
                }

                // Verify user exists
                var user = await _userRepository.GetById(createDto.AskBy);
                if (user == null)
                {
                    return BadRequest($"User with ID {createDto.AskBy} not found.");
                }

                var question = new ForumQuestion
                {
                    AskBy = createDto.AskBy,
                    ForumId = createDto.ForumId,
                    Title = createDto.Title,
                    Description = createDto.Description,
                    Question = createDto.Question,
                    CreatedAt = DateTime.UtcNow
                };

                await _forumQuestionRepository.Add(question);

                // Get current user ID to include like status
                var currentUserId = GetCurrentUserId();
                var questionDto = await MapToQuestionDto(question, currentUserId);
                return CreatedAtAction(nameof(GetForumQuestion), new { id = question.FqId }, questionDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/ForumQuestions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForumQuestion(int id, [FromBody] ForumQuestionUpdateDto updateDto)
        {
            try
            {
                if (id != updateDto.FqId)
                {
                    return BadRequest("Question ID mismatch.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingQuestion = await _forumQuestionRepository.GetById(id);
                if (existingQuestion == null)
                {
                    return NotFound($"Forum question with ID {id} not found.");
                }

                existingQuestion.Title = updateDto.Title;
                existingQuestion.Description = updateDto.Description;
                existingQuestion.Question = updateDto.Question;

                await _forumQuestionRepository.Update(existingQuestion);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ForumQuestions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForumQuestion(int id)
        {
            try
            {
                var question = await _forumQuestionRepository.GetById(id);
                if (question == null)
                {
                    return NotFound($"Forum question with ID {id} not found.");
                }

                await _forumQuestionRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/search?title=keyword&forumId=1
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ForumQuestionSummaryDto>>> SearchQuestions(
            [FromQuery] string? title, 
            [FromQuery] int? forumId,
            [FromQuery] int? userId)
        {
            try
            {
                var questions = await _forumQuestionRepository.GetAll();
                
                if (!string.IsNullOrEmpty(title))
                {
                    questions = questions.Where(q => 
                        q.Title.Contains(title, StringComparison.OrdinalIgnoreCase) ||
                        q.Description.Contains(title, StringComparison.OrdinalIgnoreCase) ||
                        q.Question.Contains(title, StringComparison.OrdinalIgnoreCase));
                }
                
                if (forumId.HasValue)
                {
                    questions = questions.Where(q => q.ForumId == forumId.Value);
                }
                
                if (userId.HasValue)
                {
                    questions = questions.Where(q => q.AskBy == userId.Value);
                }

                var questionSummaries = new List<ForumQuestionSummaryDto>();

                foreach (var question in questions.OrderByDescending(q => q.CreatedAt))
                {
                    var user = await _userRepository.GetById(question.AskBy);
                    var answers = await _answerQuestionRepository.GetByQuestionId(question.FqId);
                    
                    // Get likes count from database
                    var likesCount = await _context.QuestionLikes
                        .CountAsync(ql => ql.FqId == question.FqId);
                    
                    questionSummaries.Add(new ForumQuestionSummaryDto
                    {
                        FqId = question.FqId,
                        Title = question.Title,
                        AskerName = user?.Name ?? "Unknown User",
                        CreatedAt = question.CreatedAt,
                        TotalAnswers = answers.Count(),
                        TotalLikes = likesCount
                    });
                }

                return Ok(questionSummaries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/5/summary
        [HttpGet("{id}/summary")]
        public async Task<ActionResult<object>> GetQuestionSummary(int id)
        {
            try
            {
                var question = await _forumQuestionRepository.GetById(id);
                if (question == null)
                {
                    return NotFound($"Forum question with ID {id} not found.");
                }

                var user = await _userRepository.GetById(question.AskBy);
                var forum = await _forumRepository.GetById(question.ForumId);
                var answers = await _answerQuestionRepository.GetByQuestionId(id);
                
                // Get likes count from database
                var likesCount = await _context.QuestionLikes
                    .CountAsync(ql => ql.FqId == id);

                var summary = new
                {
                    Question = new
                    {
                        question.FqId,
                        question.Title,
                        question.Description,
                        question.Question,
                        question.CreatedAt
                    },
                    Asker = new
                    {
                        Id = user?.UserId,
                        Name = user?.Name ?? "Unknown User",
                        Email = user?.Email
                    },
                    Forum = new
                    {
                        forum?.ForumId,
                        forum?.Title
                    },
                    Statistics = new
                    {
                        TotalAnswers = answers.Count(),
                        TotalLikes = likesCount,
                        RecentAnswers = answers
                            .OrderByDescending(a => a.CreatedAt)
                            .Take(3)
                            .Select(a => new
                            {
                                a.AnswerId,
                                a.Answer,
                                a.CreatedAt,
                                a.AnswerBy
                            })
                    }
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to map ForumQuestion to ForumQuestionDto
        private async Task<ForumQuestionDto> MapToQuestionDto(ForumQuestion question, int? currentUserId = null)
        {
            var user = await _userRepository.GetById(question.AskBy);
            var forum = await _forumRepository.GetById(question.ForumId);
            var answers = await _answerQuestionRepository.GetByQuestionId(question.FqId);
            
            // Get likes count from database
            var likesCount = await _context.QuestionLikes
                .CountAsync(ql => ql.FqId == question.FqId);

            // Check if current user liked this question
            bool isLikedByCurrentUser = false;
            if (currentUserId.HasValue)
            {
                isLikedByCurrentUser = await _context.QuestionLikes
                    .AnyAsync(ql => ql.FqId == question.FqId && ql.LikedBy == currentUserId.Value);
            }

            return new ForumQuestionDto
            {
                FqId = question.FqId,
                AskBy = question.AskBy,
                ForumId = question.ForumId,
                Title = question.Title,
                Description = question.Description,
                Question = question.Question,
                CreatedAt = question.CreatedAt,
                AskerName = user?.Name ?? "Unknown User",
                AskerEmail = user?.Email,
                ForumTitle = forum?.Title ?? "Unknown Forum",
                TotalAnswers = answers.Count(),
                TotalLikes = likesCount,
                IsLikedByCurrentUser = isLikedByCurrentUser
            };
        }

        // Helper method to get current user ID from claims
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;
            return userId;
        }

        // GET: api/ForumQuestions/forum/{forumId}/paginated
        [HttpGet("forum/{forumId}/paginated")]
        public async Task<ActionResult<PaginatedForumQuestionsDto>> GetForumQuestionsPaginated(
            int forumId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                // Validate forum exists
                var forum = await _context.Forums.FindAsync(forumId);
                if (forum == null)
                {
                    return NotFound($"Forum with ID {forumId} not found.");
                }

                // Build query with search functionality
                var query = _context.ForumQuestions
                    .Include(fq => fq.AskByNavigation)
                    .Include(fq => fq.AnswerQuestions)
                        .ThenInclude(aq => aq.AnswerByNavigation)
                    .Include(fq => fq.QuestionLikes)
                    .Where(fq => fq.ForumId == forumId);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(fq => 
                        fq.Title.Contains(search) || 
                        fq.Description.Contains(search) || 
                        fq.Question.Contains(search));
                }

                // Get total count for pagination
                var totalItems = await query.CountAsync();

                // Calculate pagination values
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;

                // Get paginated results
                var forumQuestions = await query
                    .OrderByDescending(fq => fq.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs with like status
                var questionDtos = new List<ForumQuestionWithAnswersDto>();

                foreach (var fq in forumQuestions)
                {
                    // Check if current user has liked this question
                    var isLikedByCurrentUser = false;
                    if (currentUserId.HasValue)
                    {
                        isLikedByCurrentUser = await _context.QuestionLikes
                            .AnyAsync(ql => ql.FqId == fq.FqId && ql.LikedBy == currentUserId.Value);
                    }

                    // Map recent answers with like status
                    var recentAnswers = new List<ForumAnswerQuestionDto>();
                    if (fq.AnswerQuestions != null)
                    {
                        foreach (var aq in fq.AnswerQuestions.OrderByDescending(aq => aq.CreatedAt).Take(2))
                        {
                            // Check if current user has liked this answer
                            var answerIsLiked = false;
                            if (currentUserId.HasValue)
                            {
                                answerIsLiked = await _context.AnswerLikes
                                    .AnyAsync(al => al.AnswerId == aq.AnswerId && al.LikedBy == currentUserId.Value);
                            }

                            recentAnswers.Add(new ForumAnswerQuestionDto
                            {
                                AnswerId = aq.AnswerId,
                                AnswerBy = aq.AnswerBy,
                                Answer = aq.Answer,
                                AnswererName = aq.AnswerByNavigation?.Name ?? "Unknown",
                                AnswererEmail = aq.AnswerByNavigation?.Email,
                                CreatedAt = aq.CreatedAt,
                                ParentAnswerId = aq.ParentAnswerId,
                                IsLikedByCurrentUser = answerIsLiked
                            });
                        }
                    }

                    questionDtos.Add(new ForumQuestionWithAnswersDto
                    {
                        FqId = fq.FqId,
                        AskBy = fq.AskBy,
                        ForumId = fq.ForumId,
                        Title = fq.Title,
                        Description = fq.Description,
                        Question = fq.Question,
                        AskerName = fq.AskByNavigation?.Name ?? "Unknown",
                        AskerEmail = fq.AskByNavigation?.Email,
                        CreatedAt = fq.CreatedAt,
                        TotalAnswers = fq.AnswerQuestions?.Count ?? 0,
                        TotalLikes = fq.QuestionLikes?.Count ?? 0,
                        RecentAnswers = recentAnswers,
                        IsLikedByCurrentUser = isLikedByCurrentUser
                    });
                }

                var result = new PaginatedForumQuestionsDto
                {
                    Questions = questionDtos,
                    TotalCount = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages,
                    SearchTerm = search
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/ForumQuestions/like
        [HttpPost("like")]
        public async Task<ActionResult<object>> ToggleQuestionLike([FromBody] QuestionLikeToggleDto toggleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify forum question exists
                var question = await _forumQuestionRepository.GetById(toggleDto.FqId);
                if (question == null)
                {
                    return NotFound($"Forum question with ID {toggleDto.FqId} not found.");
                }

                // Verify user exists
                var user = await _userRepository.GetById(toggleDto.UserId);
                if (user == null)
                {
                    return BadRequest($"User with ID {toggleDto.UserId} not found.");
                }

                // Check if user already liked this question
                var existingLike = await _context.QuestionLikes
                    .FirstOrDefaultAsync(ql => ql.FqId == toggleDto.FqId && ql.LikedBy == toggleDto.UserId);

                bool isLiked;
                if (existingLike != null)
                {
                    // Unlike - remove the like
                    _context.QuestionLikes.Remove(existingLike);
                    isLiked = false;
                }
                else
                {
                    // Like - add new like
                    var newLike = new QuestionLike
                    {
                        FqId = toggleDto.FqId,
                        LikedBy = toggleDto.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.QuestionLikes.Add(newLike);
                    isLiked = true;
                }

                await _context.SaveChangesAsync();

                // Get updated like count
                var totalLikes = await _context.QuestionLikes
                    .CountAsync(ql => ql.FqId == toggleDto.FqId);

                return Ok(new
                {
                    FqId = toggleDto.FqId,
                    IsLiked = isLiked,
                    TotalLikes = totalLikes,
                    Message = isLiked ? "Question liked successfully" : "Question unliked successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/{fqId}/likes
        [HttpGet("{fqId}/likes")]
        public async Task<ActionResult<QuestionLikeStatsDto>> GetQuestionLikes(int fqId, [FromQuery] int? currentUserId = null)
        {
            try
            {
                // Verify forum question exists
                var question = await _forumQuestionRepository.GetById(fqId);
                if (question == null)
                {
                    return NotFound($"Forum question with ID {fqId} not found.");
                }

                // Get all likes for this question
                var likes = await _context.QuestionLikes
                    .Include(ql => ql.LikedByNavigation)
                    .Where(ql => ql.FqId == fqId)
                    .OrderByDescending(ql => ql.CreatedAt)
                    .ToListAsync();

                // Check if current user liked this question
                bool isLikedByCurrentUser = false;
                if (currentUserId.HasValue)
                {
                    isLikedByCurrentUser = likes.Any(l => l.LikedBy == currentUserId.Value);
                }

                // Map to DTOs
                var likeDtos = likes.Take(10).Select(l => new QuestionLikeDto
                {
                    LikeId = l.LikeId,
                    FqId = l.FqId,
                    LikedBy = l.LikedBy,
                    CreatedAt = l.CreatedAt,
                    LikerName = l.LikedByNavigation?.Name ?? "Unknown User",
                    LikerEmail = l.LikedByNavigation?.Email,
                    QuestionTitle = question.Title
                }).ToList();

                var result = new QuestionLikeStatsDto
                {
                    FqId = fqId,
                    TotalLikes = likes.Count,
                    IsLikedByCurrentUser = isLikedByCurrentUser,
                    RecentLikes = likeDtos
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/user/{userId}/likes
        [HttpGet("user/{userId}/likes")]
        public async Task<ActionResult<IEnumerable<QuestionLikeDto>>> GetUserQuestionLikes(int userId)
        {
            try
            {
                // Verify user exists
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                // Get all question likes by this user
                var userLikes = await _context.QuestionLikes
                    .Include(ql => ql.Fq)
                    .Include(ql => ql.LikedByNavigation)
                    .Where(ql => ql.LikedBy == userId)
                    .OrderByDescending(ql => ql.CreatedAt)
                    .ToListAsync();

                var likeDtos = userLikes.Select(l => new QuestionLikeDto
                {
                    LikeId = l.LikeId,
                    FqId = l.FqId,
                    LikedBy = l.LikedBy,
                    CreatedAt = l.CreatedAt,
                    LikerName = l.LikedByNavigation?.Name ?? "Unknown User",
                    LikerEmail = l.LikedByNavigation?.Email,
                    QuestionTitle = l.Fq?.Title ?? "Unknown Question"
                }).ToList();

                return Ok(likeDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/ForumQuestions/{fqId}/likes/{userId}
        [HttpDelete("{fqId}/likes/{userId}")]
        public async Task<IActionResult> RemoveQuestionLike(int fqId, int userId)
        {
            try
            {
                // Find the like
                var like = await _context.QuestionLikes
                    .FirstOrDefaultAsync(ql => ql.FqId == fqId && ql.LikedBy == userId);

                if (like == null)
                {
                    return NotFound("Like not found.");
                }

                _context.QuestionLikes.Remove(like);
                await _context.SaveChangesAsync();

                // Get updated like count
                var totalLikes = await _context.QuestionLikes
                    .CountAsync(ql => ql.FqId == fqId);

                return Ok(new
                {
                    FqId = fqId,
                    TotalLikes = totalLikes,
                    Message = "Like removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ForumQuestions/popular
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopularQuestions([FromQuery] int? forumId = null, [FromQuery] int limit = 10)
        {
            try
            {
                var query = _context.ForumQuestions
                    .Include(fq => fq.AskByNavigation)
                    .Include(fq => fq.QuestionLikes)
                    .AsQueryable();

                if (forumId.HasValue)
                {
                    query = query.Where(fq => fq.ForumId == forumId.Value);
                }

                var popularQuestions = await query
                    .Where(fq => fq.QuestionLikes.Any()) // Only questions with likes
                    .OrderByDescending(fq => fq.QuestionLikes.Count)
                    .ThenByDescending(fq => fq.CreatedAt)
                    .Take(limit)
                    .Select(fq => new
                    {
                        fq.FqId,
                        fq.Title,
                        fq.Description,
                        AskerName = fq.AskByNavigation!.Name,
                        fq.CreatedAt,
                        TotalLikes = fq.QuestionLikes.Count,
                        fq.ForumId
                    })
                    .ToListAsync();

                return Ok(popularQuestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
