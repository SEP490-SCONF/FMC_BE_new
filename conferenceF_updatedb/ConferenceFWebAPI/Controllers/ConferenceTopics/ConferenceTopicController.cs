using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.ConferenceTopics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConferenceFWebAPI.Controllers.ConferenceTopics
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferenceTopicController : ControllerBase
    {
        private readonly ConferenceFTestContext _context;
        private readonly IMapper _mapper;

        public ConferenceTopicController(ConferenceFTestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/ConferenceTopic
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConferenceTopicDTO>>> GetAll()
        {
            var entities = await _context.Set<Dictionary<string, object>>("ConferenceTopic").ToListAsync();

            var topicIds = entities.Select(e => (int)e["TopicId"]).Distinct();
            var topics = await _context.Topics
                .Where(t => topicIds.Contains(t.TopicId))
                .ToDictionaryAsync(t => t.TopicId, t => t.TopicName);

            var result = entities.Select(e =>
            {
                int topicId = (int)e["TopicId"];
                topics.TryGetValue(topicId, out string? topicName);

                return new ConferenceTopicDTO
                {
                    ConferenceId = (int)e["ConferenceId"],
                    TopicId = topicId,
                    TopicName = topicName
                };
            }).ToList();

            return Ok(result);
        }
            [HttpGet("conference/{conferenceId}")]
            public async Task<ActionResult<IEnumerable<ConferenceTopicDTO>>> GetByConferenceId(int conferenceId)
            {
                // Lấy tất cả các dòng liên kết từ bảng trung gian ConferenceTopic theo ConferenceId
                var entities = await _context.Set<Dictionary<string, object>>("ConferenceTopic")
                    .Where(e => (int)e["ConferenceId"] == conferenceId)
                    .ToListAsync();

                if (!entities.Any())
                    return NotFound("Không có chủ đề nào cho hội thảo này.");

                var topicIds = entities.Select(e => (int)e["TopicId"]).Distinct();
                var topics = await _context.Topics
                    .Where(t => topicIds.Contains(t.TopicId))
                    .ToDictionaryAsync(t => t.TopicId, t => t.TopicName);

                var result = entities.Select(e =>
                {
                    int topicId = (int)e["TopicId"];
                    topics.TryGetValue(topicId, out string? topicName);

                    return new ConferenceTopicDTO
                    {
                        ConferenceId = conferenceId,
                        TopicId = topicId,
                        TopicName = topicName
                    };
                }).ToList();

                return Ok(result);
            }


        // POST: api/ConferenceTopic
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConferenceTopicDTO dto)
        {
            var conferenceExists = await _context.Conferences.AnyAsync(c => c.ConferenceId == dto.ConferenceId);
            var topicExists = await _context.Topics.AnyAsync(t => t.TopicId == dto.TopicId);

            if (!conferenceExists || !topicExists)
                return BadRequest("ConferenceId hoặc TopicId không tồn tại.");

            // Kiểm tra trùng lặp
            var exists = await _context.Set<Dictionary<string, object>>("ConferenceTopic")
                .AnyAsync(e =>
                    (int)e["ConferenceId"] == dto.ConferenceId &&
                    (int)e["TopicId"] == dto.TopicId);

            if (exists)
                return Conflict("ConferenceTopic đã tồn tại.");

            var entity = _mapper.Map<Dictionary<string, object>>(dto);
            _context.Set<Dictionary<string, object>>("ConferenceTopic").Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), dto);
        }

        // DELETE: api/ConferenceTopic/1/2
        [HttpDelete("{conferenceId}/{topicId}")]
        public async Task<IActionResult> Delete(int conferenceId, int topicId)
        {
            var entity = await _context.Set<Dictionary<string, object>>("ConferenceTopic")
                .FirstOrDefaultAsync(e =>
                    (int)e["ConferenceId"] == conferenceId &&
                    (int)e["TopicId"] == topicId);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
