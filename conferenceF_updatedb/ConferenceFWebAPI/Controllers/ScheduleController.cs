using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Schedules;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPaperRepository _paperRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ScheduleController(IScheduleRepository scheduleRepository, IPaperRepository paperRepository, IUserRepository userRepository, IMapper mapper)
        {
            _scheduleRepository = scheduleRepository;
            _paperRepository = paperRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // POST: api/Schedule/add
        [HttpPost("add")]
        public async Task<IActionResult> AddSchedule([FromForm] ScheduleRequestDto request)
        {
            // Kiểm tra null trước khi so sánh
            if (request.TimelineId <= 0 ||  !request.PresentationStartTime.HasValue || !request.PresentationEndTime.HasValue)
            {
                return BadRequest("Timeline, and time range are required.");
            }


            Paper? paper = null;
            if (request.PaperId.HasValue)
            {
                paper = await _paperRepository.GetPaperByIdAsync(request.PaperId.Value);
                if (paper == null || paper.Status != "Accepted" || paper.IsPresented == false)
                    return BadRequest("The paper is not ready to be scheduled for presentation.");
            }

            User? presenter = null;
            if (request.PresenterId.HasValue)
            {
                presenter = await _userRepository.GetById(request.PresenterId.Value);
                if (presenter == null)
                    return NotFound("Presenter not found.");
            }


            var newSchedule = new Schedule
            {
                TimeLineId = request.TimelineId,
                ConferenceId = request.ConferenceId,
                PaperId = request.PaperId,
                PresenterId = request.PresenterId,
                SessionTitle = request.SessionTitle,
                Location = request.Location,
                PresentationStartTime = request.PresentationStartTime,
                PresentationEndTime = request.PresentationEndTime
            };

            try
            {
                var addedSchedule = await _scheduleRepository.AddScheduleAsync(newSchedule);
                // Lấy lại đầy đủ thông tin để trả về DTO
                var fullSchedule = await _scheduleRepository.GetScheduleByIdAsync(addedSchedule.ScheduleId);
                var scheduleDto = _mapper.Map<ScheduleRequestDto>(fullSchedule);
                return Ok(scheduleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Schedule/{scheduleId}
        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetScheduleById(int scheduleId)
        {
            var schedule = await _scheduleRepository.GetScheduleByIdAsync(scheduleId);
            if (schedule == null)
                return NotFound($"Schedule with ID {scheduleId} not found.");

            var scheduleDto = _mapper.Map<ScheduleResponseDto>(schedule);
            return Ok(scheduleDto);
        }

        // GET: api/Schedule/conference/{conferenceId}
        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetSchedulesByConference(int conferenceId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByConferenceIdAsync(conferenceId);
            if (schedules == null || !schedules.Any())
                return NotFound("No schedules found for this conference.");

            var scheduleDtos = _mapper.Map<List<ScheduleResponseDto>>(schedules);
            return Ok(scheduleDtos);
        }

        // PUT: api/Schedule/edit/{scheduleId}
        [HttpPut("edit/{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromForm] ScheduleUpdateDto request)
        {
            Paper? paper = null;
            if (request.PaperId.HasValue)
            {
                paper = await _paperRepository.GetPaperByIdAsync(request.PaperId.Value);
                if (paper == null || paper.Status != "Accepted" || paper.IsPresented == false)
                    return BadRequest("The paper is not ready to be scheduled for presentation.");
            }

            User? presenter = null;
            if (request.PresenterId.HasValue)
            {
                presenter = await _userRepository.GetById(request.PresenterId.Value);
                if (presenter == null)
                    return NotFound("Presenter not found.");
            }

            var existingSchedule = await _scheduleRepository.GetScheduleByIdAsync(scheduleId);
            if (existingSchedule == null)
                return NotFound($"Schedule with ID {scheduleId} not found.");

            // Map chỉ các trường có giá trị không null
            _mapper.Map(request, existingSchedule);

            try
            {
                await _scheduleRepository.UpdateScheduleAsync(existingSchedule);
                var updatedDto = _mapper.Map<ScheduleUpdateDto>(existingSchedule);
                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                // In ra cả message và inner exception để debug
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, $"Internal server error: {ex.Message} | Inner: {innerMessage}");
            }

        }


        // DELETE: api/Schedule/delete/{scheduleId}
        [HttpDelete("delete/{scheduleId}")]
        public async Task<IActionResult> DeleteSchedule(int scheduleId)
        {
            try
            {
                await _scheduleRepository.DeleteScheduleAsync(scheduleId);
                return Ok(new { Message = "Schedule deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("timeline/{timelineId}")]
        public async Task<IActionResult> GetSchedulesByTimeline(int timelineId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByTimelineIdAsync(timelineId);
            if (schedules == null || !schedules.Any())
                return NotFound($"No schedules found for timeline ID {timelineId}.");

            var scheduleDtos = _mapper.Map<List<ScheduleResponseDto>>(schedules);
            return Ok(scheduleDtos);
        }
    }
}
