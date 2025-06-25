using Microsoft.AspNetCore.Mvc;
using Repository;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using AutoMapper;

namespace FMC_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicsController : ControllerBase
    {
        private readonly ITopicRepository _topicRepository;
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IMapper _mapper;

        public TopicsController(ITopicRepository topicRepository, IMapper mapper, IConferenceRepository conferenceRepository)
        {
            _topicRepository = topicRepository;
            _mapper = mapper;
            _conferenceRepository = conferenceRepository;
        }

        // GET: api/Topics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicDTO>>> GetAll()
        {
            var topics = await _topicRepository.GetAll();
            var result = _mapper.Map<IEnumerable<TopicDTO>>(topics);
            return Ok(result);
        }

        // GET: api/Topics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TopicDTO>> GetById(int id)
        {
            var topic = await _topicRepository.GetById(id);
            if (topic == null)
            {
                return NotFound($"Topic with ID {id} not found.");
            }

            return Ok(_mapper.Map<TopicDTO>(topic));
        }

        // POST: api/Topics
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AddOrUpdateTopicDTO topicDto)
        {
            if (topicDto.TopicName == null)
            {
                return BadRequest("Topic Name is requied.");
            }

            var topic = _mapper.Map<Topic>(topicDto);
            await _topicRepository.Add(topic);

            return CreatedAtAction(nameof(GetById), new { id = topic.TopicId }, topicDto);
        }

        // PUT: api/Topics/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] AddOrUpdateTopicDTO topicDto)
        {
            if (id != topicDto.TopicId)
            {
                return BadRequest("Topic ID mismatch.");
            }
            try
            {
                if(topicDto.TopicId != null)
                {
                    var topic = await _topicRepository.GetById((int)topicDto.TopicId);
               
                _mapper.Map<Topic>(topicDto);
                await _topicRepository.Update(topic);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE: api/Topics/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var topic = await _topicRepository.GetById(id);
                if (topic == null)
                {
                    return NotFound($"Topic with ID {id} not found.");
                }

                await _topicRepository.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting: {ex.Message}");
            }
        }
    }
}
