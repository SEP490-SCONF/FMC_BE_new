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
    public class ForumQuestionsController : ControllerBase
    {
        private readonly IForumQuestionRepository _forumQuestionRepository;
        private readonly IForumRepository _forumRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAnswerQuestionRepository _answerQuestionRepository;
        private readonly IQuestionLikeRepository _questionLikeRepository;
        private readonly IMapper _mapper;

        public ForumQuestionsController(
            IForumQuestionRepository forumQuestionRepository,
            IForumRepository forumRepository,
            IUserRepository userRepository,
            IAnswerQuestionRepository answerQuestionRepository,
            IQuestionLikeRepository questionLikeRepository,
            IMapper mapper)
        {
            _forumQuestionRepository = forumQuestionRepository;
            _forumRepository = forumRepository;
            _userRepository = userRepository;
            _answerQuestionRepository = answerQuestionRepository;
            _questionLikeRepository = questionLikeRepository;
            _mapper = mapper;
        }

        // GET: api/ForumQuestions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ForumQuestionDto>>> GetForumQuestions()
        {
            try
            {
                var questions = await _forumQuestionRepository.GetAll();
                var questionDtos = new List<ForumQuestionDto>();

                foreach (var question in questions)
                {
                    var questionDto = await MapToQuestionDto(question);
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

                var questionDto = await MapToQuestionDto(question);
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
                var questions = await _forumQuestionRepository.GetByForumId(forumId);
                var questionDtos = new List<ForumQuestionDto>();

                foreach (var question in questions)
                {
                    var questionDto = await MapToQuestionDto(question);
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
                var allQuestions = await _forumQuestionRepository.GetAll();
                var userQuestions = allQuestions.Where(q => q.AskBy == userId);
                var questionDtos = new List<ForumQuestionDto>();

                foreach (var question in userQuestions)
                {
                    var questionDto = await MapToQuestionDto(question);
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

                var questionDto = await MapToQuestionDto(question);
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
                    var likes = await _questionLikeRepository.GetByQuestionId(question.FqId);
                    
                    questionSummaries.Add(new ForumQuestionSummaryDto
                    {
                        FqId = question.FqId,
                        Title = question.Title,
                        AskerName = user?.Name ?? "Unknown User",
                        CreatedAt = question.CreatedAt,
                        TotalAnswers = answers.Count(),
                        TotalLikes = likes.Count()
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
                var likes = await _questionLikeRepository.GetByQuestionId(id);

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
                        TotalLikes = likes.Count(),
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
        private async Task<ForumQuestionDto> MapToQuestionDto(ForumQuestion question)
        {
            var user = await _userRepository.GetById(question.AskBy);
            var forum = await _forumRepository.GetById(question.ForumId);
            var answers = await _answerQuestionRepository.GetByQuestionId(question.FqId);
            var likes = await _questionLikeRepository.GetByQuestionId(question.FqId);

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
                TotalLikes = likes.Count()
            };
        }
    }
}
