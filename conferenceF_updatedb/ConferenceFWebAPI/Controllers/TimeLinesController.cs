using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.TimeLines;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Hangfire;


namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TimeLinesController : ControllerBase
    {
        private readonly ITimeLineRepository _timeLineRepository;
        // KHÔNG CẦN inject IBackgroundJobClient nữa
        // private readonly IBackgroundJobClient _backgroundJobClient; 

        public TimeLinesController(ITimeLineRepository timeLineRepository) // Bỏ IBackgroundJobClient khỏi constructor
        {
            _timeLineRepository = timeLineRepository;
            // _backgroundJobClient = backgroundJobClient; // Bỏ dòng này
        }

        // GET: api/timelines/conference/{conferenceId}
        [HttpGet("conference/{conferenceId}")]
        public async Task<ActionResult<IEnumerable<TimeLine>>> GetTimeLinesByConference(int conferenceId)
        {
            var timeLines = await _timeLineRepository.GetTimeLinesByConferenceAsync(conferenceId);
            return Ok(timeLines);
        }

        // POST: api/timelines
        [HttpPost]
        public async Task<ActionResult<TimeLine>> CreateTimeLine([FromBody] TimeLineCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newTimeLine = new TimeLine
            {
                ConferenceId = createDto.ConferenceId,
                Date = createDto.Date.ToUniversalTime(), // Luôn dùng UTC cho server
                Description = createDto.Description,
                HangfireJobId = null // Job ID sẽ được gán bởi BackgroundService sau
            };

            var createdTimeLine = await _timeLineRepository.CreateTimeLineAsync(newTimeLine);

            // BackgroundService sẽ tự động phát hiện và lên lịch job cho timeline mới này.

            return CreatedAtAction(nameof(GetTimeLinesByConference), new { conferenceId = createdTimeLine.ConferenceId }, createdTimeLine);
        }

        // PUT: api/timelines/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeLine(int id, [FromBody] TimeLineUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTimeLine = await _timeLineRepository.GetTimeLineByIdAsync(id);
            if (existingTimeLine == null)
            {
                return NotFound();
            }
            existingTimeLine.Date = updateDto.Date.ToUniversalTime();
            existingTimeLine.Description = updateDto.Description;
            existingTimeLine.HangfireJobId = null; // Đây là điểm quan trọng: Reset để background service biết cần lên lịch lại

            var success = await _timeLineRepository.UpdateTimeLineAsync(existingTimeLine);

            if (!success)
            {
                return NotFound(); // Có thể xảy ra nếu có race condition
            }

            // BackgroundService sẽ tự động phát hiện sự thay đổi và cập nhật/reschedule job.

            return NoContent(); // HTTP 204: Thành công và không có nội dung trả về
        }
    }
}
