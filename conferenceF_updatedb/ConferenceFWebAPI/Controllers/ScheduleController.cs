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
        public async Task<IActionResult> AddSchedule([FromBody] ScheduleRequestDto request)
        {
            // Kiểm tra tính hợp lệ của request
            if (request.PaperId <= 0 || request.PresenterId <= 0 || request.ConferenceId <= 0)
            {
                return BadRequest("Invalid data provided.");
            }

            // Kiểm tra xem bài báo đã được chấp nhận và chọn thuyết trình chưa
            var paper = await _paperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null || paper.Status != "Accepted" || paper.IsPresented == false)
            {
                return BadRequest("The paper is not ready to be scheduled for presentation.");
            }

            // Kiểm tra xem người thuyết trình có tồn tại không
            var presenter = await _userRepository.GetById(request.PresenterId);
            if (presenter == null)
            {
                return NotFound("Presenter not found.");
            }

            var newSchedule = new Schedule
            {
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
                return Ok(addedSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetSchedulesByConference(int conferenceId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByConferenceIdAsync(conferenceId);

            if (schedules == null || !schedules.Any())
            {
                return NotFound("No schedules found for this conference.");
            }

            // Ánh xạ danh sách entity sang danh sách DTO
            var scheduleDtos = _mapper.Map<List<ScheduleRequestDto>>(schedules);

            return Ok(scheduleDtos);
        }

        // PUT: api/Schedule/edit/1
        [HttpPut("edit/{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] ScheduleUpdateDto request)
        {
            var existingSchedule = await _scheduleRepository.GetScheduleByIdAsync(scheduleId);
            if (existingSchedule == null)
            {
                return NotFound($"Schedule with ID {scheduleId} not found.");
            }

            existingSchedule.SessionTitle = request.SessionTitle;
            existingSchedule.Location = request.Location;
            existingSchedule.PresentationStartTime = request.PresentationStartTime;
            existingSchedule.PresentationEndTime = request.PresentationEndTime;

            try
            {
                await _scheduleRepository.UpdateScheduleAsync(existingSchedule);
                return Ok(new { Message = "Schedule updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Schedule/delete/1
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
    }
}
