using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.AnswerQuestions;
using AutoMapper;
using Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ConferenceFWebAPI.Hubs;

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
        private readonly INotificationRepository _notificationRepository; // Add this repository
        private readonly IHubContext<NotificationHub> _hubContext;


        public AnswerQuestionsController(
            IAnswerQuestionRepository answerQuestionRepository,
            IForumQuestionRepository forumQuestionRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ConferenceFTestContext context,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _answerQuestionRepository = answerQuestionRepository;
            _forumQuestionRepository = forumQuestionRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        // GET: api/AnswerQuestions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerQuestionDto>>> GetAnswerQuestions()
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var answers = await _answerQuestionRepository.GetAll();
                var answerDtos = new List<AnswerQuestionDto>();

                foreach (var answer in answers)
                {
                    var answerDto = await MapToAnswerDto(answer, currentUserId);
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
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var answer = await _answerQuestionRepository.GetById(id);
                if (answer == null)
                {
                    return NotFound($"Answer question with ID {id} not found.");
                }

                var answerDto = await MapToAnswerDto(answer, currentUserId);
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

                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();

                // Map to DTOs
                var answerDtos = new List<AnswerQuestionDto>();
                foreach (var aq in answerQuestions)
                {
                    // Check if current user liked this answer
                    bool isLikedByCurrentUser = false;
                    if (currentUserId.HasValue)
                    {
                        isLikedByCurrentUser = await _context.AnswerLikes
                            .AnyAsync(al => al.AnswerId == aq.AnswerId && al.LikedBy == currentUserId.Value);
                    }

                    answerDtos.Add(new AnswerQuestionDto
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
                        ParentAnswererName = aq.ParentAnswer?.AnswerByNavigation?.Name,
                        IsLikedByCurrentUser = isLikedByCurrentUser
                    });
                }

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
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var answers = await _answerQuestionRepository.GetByQuestionId(forumQuestionId);
                var answerDtos = new List<AnswerQuestionDto>();

                foreach (var answer in answers)
                {
                    var answerDto = await MapToAnswerDto(answer, currentUserId);
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
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
                var allAnswers = await _answerQuestionRepository.GetAll();
                var userAnswers = allAnswers.Where(a => a.AnswerBy == userId);
                var answerDtos = new List<AnswerQuestionDto>();

                foreach (var answer in userAnswers)
                {
                    var answerDto = await MapToAnswerDto(answer, currentUserId);
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

                // 1. Kiểm tra sự tồn tại của câu hỏi và người dùng
                var forumQuestion = await _forumQuestionRepository.GetById(createDto.FqId);
                if (forumQuestion == null)
                {
                    return BadRequest($"Forum question with ID {createDto.FqId} not found.");
                }

                var user = await _userRepository.GetById(createDto.AnswerBy);
                if (user == null)
                {
                    return BadRequest($"User with ID {createDto.AnswerBy} not found.");
                }

                // 2. Kiểm tra câu trả lời cha (nếu có)
                if (createDto.ParentAnswerId.HasValue)
                {
                    var parentAnswer = await _answerQuestionRepository.GetById(createDto.ParentAnswerId.Value);
                    if (parentAnswer == null || parentAnswer.FqId != createDto.FqId)
                    {
                        return BadRequest("Parent answer must exist and belong to the same forum question.");
                    }
                }

                // 3. Tạo câu trả lời mới
                var answerQuestion = new AnswerQuestion
                {
                    FqId = createDto.FqId,
                    AnswerBy = createDto.AnswerBy,
                    ParentAnswerId = createDto.ParentAnswerId,
                    Answer = createDto.Answer,
                    CreatedAt = DateTime.UtcNow
                };

                await _answerQuestionRepository.Add(answerQuestion);

                // 4. Bắt đầu logic thông báo
                var originalQuestionerId = forumQuestion.AskBy;
                var notificationTitle = $"Has answer for your question '{forumQuestion.Title}'";
                var notificationContent = $"{user.Name} has reply your question.";

                // 5. Chỉ gửi thông báo cho người đặt câu hỏi gốc
                if (originalQuestionerId != createDto.AnswerBy)
                {
                    // Tạo bản ghi thông báo trong cơ sở dữ liệu
                    var notification = new Notification
                    {
                        Title = notificationTitle,
                        Content = notificationContent,
                        UserId = originalQuestionerId,
                        RoleTarget = "User",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddNotificationAsync(notification);

                    // Gửi thông báo real-time qua SignalR
                    await _hubContext.Clients.User(originalQuestionerId.ToString()).SendAsync("ReceiveNotification", notificationTitle, notificationContent);
                }

                var currentUserId = GetCurrentUserId();
                var answerDto = await MapToAnswerDto(answerQuestion, currentUserId);
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
                // Get current user ID from claims
                var currentUserId = GetCurrentUserId();
                
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
                    
                    // Get likes count from database
                    var likesCount = await _context.AnswerLikes
                        .CountAsync(al => al.AnswerId == answer.AnswerId);

                    // Check if current user has liked this answer
                    var isLikedByCurrentUser = false;
                    if (currentUserId.HasValue)
                    {
                        isLikedByCurrentUser = await _context.AnswerLikes
                            .AnyAsync(al => al.AnswerId == answer.AnswerId && al.LikedBy == currentUserId.Value);
                    }

                    answerSummaries.Add(new AnswerQuestionSummaryDto
                    {
                        AnswerId = answer.AnswerId,
                        Answer = answer.Answer.Length > 200 ? answer.Answer.Substring(0, 200) + "..." : answer.Answer,
                        AnswererName = user?.Name ?? "Unknown User",
                        CreatedAt = answer.CreatedAt,
                        TotalLikes = likesCount,
                        TotalReplies = replies.Count(),
                        IsReply = answer.ParentAnswerId.HasValue,
                        IsLikedByCurrentUser = isLikedByCurrentUser
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
        private async Task<AnswerQuestionDto> MapToAnswerDto(AnswerQuestion answer, int? currentUserId = null)
        {
            var user = await _userRepository.GetById(answer.AnswerBy);
            var forumQuestion = await _forumQuestionRepository.GetById(answer.FqId);
            var allAnswers = await _answerQuestionRepository.GetAll();
            var replies = allAnswers.Where(a => a.ParentAnswerId == answer.AnswerId);
            
            // Get likes count from database
            var likesCount = await _context.AnswerLikes
                .CountAsync(al => al.AnswerId == answer.AnswerId);
            
            // Check if current user liked this answer
            bool isLikedByCurrentUser = false;
            if (currentUserId.HasValue)
            {
                isLikedByCurrentUser = await _context.AnswerLikes
                    .AnyAsync(al => al.AnswerId == answer.AnswerId && al.LikedBy == currentUserId.Value);
            }
            
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
                TotalLikes = likesCount,
                HasReplies = replies.Any(),
                TotalReplies = replies.Count(),
                ParentAnswerText = parentAnswer?.Answer?.Length > 100 
                    ? parentAnswer.Answer.Substring(0, 100) + "..." 
                    : parentAnswer?.Answer,
                ParentAnswererName = parentAnswerer?.Name,
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

        // POST: api/AnswerQuestions/like
        [HttpPost("like")]
        public async Task<ActionResult<object>> ToggleLike([FromBody] AnswerLikeToggleDto toggleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify answer exists
                var answer = await _answerQuestionRepository.GetById(toggleDto.AnswerId);
                if (answer == null)
                {
                    return NotFound($"Answer with ID {toggleDto.AnswerId} not found.");
                }

                // Verify user exists
                var user = await _userRepository.GetById(toggleDto.UserId);
                if (user == null)
                {
                    return BadRequest($"User with ID {toggleDto.UserId} not found.");
                }

                // Check if user already liked this answer
                var existingLike = await _context.AnswerLikes
                    .FirstOrDefaultAsync(al => al.AnswerId == toggleDto.AnswerId && al.LikedBy == toggleDto.UserId);

                bool isLiked;
                if (existingLike != null)
                {
                    // Unlike - remove the like
                    _context.AnswerLikes.Remove(existingLike);
                    isLiked = false;
                }
                else
                {
                    // Like - add new like
                    var newLike = new AnswerLike
                    {
                        AnswerId = toggleDto.AnswerId,
                        LikedBy = toggleDto.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AnswerLikes.Add(newLike);
                    isLiked = true;
                }

                await _context.SaveChangesAsync();

                // Get updated like count
                var totalLikes = await _context.AnswerLikes
                    .CountAsync(al => al.AnswerId == toggleDto.AnswerId);

                return Ok(new
                {
                    AnswerId = toggleDto.AnswerId,
                    IsLiked = isLiked,
                    TotalLikes = totalLikes,
                    Message = isLiked ? "Answer liked successfully" : "Answer unliked successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/{answerId}/likes
        [HttpGet("{answerId}/likes")]
        public async Task<ActionResult<AnswerLikeStatsDto>> GetAnswerLikes(int answerId, [FromQuery] int? currentUserId = null)
        {
            try
            {
                // Verify answer exists
                var answer = await _answerQuestionRepository.GetById(answerId);
                if (answer == null)
                {
                    return NotFound($"Answer with ID {answerId} not found.");
                }

                // Get all likes for this answer
                var likes = await _context.AnswerLikes
                    .Include(al => al.LikedByNavigation)
                    .Where(al => al.AnswerId == answerId)
                    .OrderByDescending(al => al.CreatedAt)
                    .ToListAsync();

                // Check if current user liked this answer
                bool isLikedByCurrentUser = false;
                if (currentUserId.HasValue)
                {
                    isLikedByCurrentUser = likes.Any(l => l.LikedBy == currentUserId.Value);
                }

                // Map to DTOs
                var likeDtos = likes.Take(10).Select(l => new AnswerLikeDto
                {
                    LikeId = l.LikeId,
                    AnswerId = l.AnswerId,
                    LikedBy = l.LikedBy,
                    CreatedAt = l.CreatedAt,
                    LikerName = l.LikedByNavigation?.Name ?? "Unknown User",
                    LikerEmail = l.LikedByNavigation?.Email
                }).ToList();

                var result = new AnswerLikeStatsDto
                {
                    AnswerId = answerId,
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

        // GET: api/AnswerQuestions/user/{userId}/likes
        [HttpGet("user/{userId}/likes")]
        public async Task<ActionResult<IEnumerable<AnswerLikeDto>>> GetUserLikes(int userId)
        {
            try
            {
                // Verify user exists
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                // Get all likes by this user
                var userLikes = await _context.AnswerLikes
                    .Include(al => al.Answer)
                    .Include(al => al.LikedByNavigation)
                    .Where(al => al.LikedBy == userId)
                    .OrderByDescending(al => al.CreatedAt)
                    .ToListAsync();

                var likeDtos = userLikes.Select(l => new AnswerLikeDto
                {
                    LikeId = l.LikeId,
                    AnswerId = l.AnswerId,
                    LikedBy = l.LikedBy,
                    CreatedAt = l.CreatedAt,
                    LikerName = l.LikedByNavigation?.Name ?? "Unknown User",
                    LikerEmail = l.LikedByNavigation?.Email,
                    AnswerContent = l.Answer?.Answer?.Length > 100 
                        ? l.Answer.Answer.Substring(0, 100) + "..." 
                        : l.Answer?.Answer
                }).ToList();

                return Ok(likeDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/AnswerQuestions/{answerId}/likes/{userId}
        [HttpDelete("{answerId}/likes/{userId}")]
        public async Task<IActionResult> RemoveLike(int answerId, int userId)
        {
            try
            {
                // Find the like
                var like = await _context.AnswerLikes
                    .FirstOrDefaultAsync(al => al.AnswerId == answerId && al.LikedBy == userId);

                if (like == null)
                {
                    return NotFound("Like not found.");
                }

                _context.AnswerLikes.Remove(like);
                await _context.SaveChangesAsync();

                // Get updated like count
                var totalLikes = await _context.AnswerLikes
                    .CountAsync(al => al.AnswerId == answerId);

                return Ok(new
                {
                    AnswerId = answerId,
                    TotalLikes = totalLikes,
                    Message = "Like removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/AnswerQuestions/popular
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopularAnswers([FromQuery] int? forumQuestionId = null, [FromQuery] int limit = 10)
        {
            try
            {
                var query = _context.AnswerQuestions
                    .Include(aq => aq.AnswerByNavigation)
                    .Include(aq => aq.AnswerLikes)
                    .AsQueryable();

                if (forumQuestionId.HasValue)
                {
                    query = query.Where(aq => aq.FqId == forumQuestionId.Value);
                }

                var popularAnswers = await query
                    .Where(aq => aq.AnswerLikes.Any()) // Only answers with likes
                    .OrderByDescending(aq => aq.AnswerLikes.Count)
                    .ThenByDescending(aq => aq.CreatedAt)
                    .Take(limit)
                    .Select(aq => new
                    {
                        aq.AnswerId,
                        aq.Answer,
                        AnswererName = aq.AnswerByNavigation!.Name,
                        aq.CreatedAt,
                        TotalLikes = aq.AnswerLikes.Count,
                        aq.FqId
                    })
                    .ToListAsync();

                return Ok(popularAnswers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
