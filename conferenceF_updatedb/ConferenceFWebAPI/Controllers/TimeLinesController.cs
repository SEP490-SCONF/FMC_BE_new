using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.TimeLines;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Hangfire;
using ConferenceFWebAPI.Service;


namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimelinesController : ControllerBase
    {
        private readonly TimeLineManager _timeLineManager;
        private readonly ITimeLineRepository _timeLineRepository; // Vẫn giữ để GetTimeLinesByConference

        // Inject TimeLineManager và ITimeLineRepository
        public TimelinesController(TimeLineManager timeLineManager, ITimeLineRepository timeLineRepository)
        {
            _timeLineManager = timeLineManager;
            _timeLineRepository = timeLineRepository;
        }

        // Endpoint chỉ để test Hangfire hoạt động
        [HttpGet("test-hangfire-job")]
        public IActionResult TestHangfireJob()
        {
            // Lên lịch một job sẽ chạy sau 10 giây
            Hangfire.BackgroundJob.Schedule(
                () => Console.WriteLine("Hangfire test job executed!"),
                TimeSpan.FromSeconds(10)
            );
            return Ok("Test job scheduled. Check Hangfire Dashboard in 10 seconds.");
        }

        /// <summary>
        /// Lấy danh sách các Timelines theo ID của Conference.
        /// </summary>
        /// <param name="conferenceId">ID của Conference.</param>
        /// <returns>Danh sách các Timeline.</returns>
        [HttpGet("conference/{conferenceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TimelineResponseDto>>> GetTimeLinesByConference(int conferenceId)
        {
            var timeLines = await _timeLineRepository.GetTimeLinesByConferenceAsync(conferenceId);
            if (timeLines == null || !timeLines.Any())
            {
                return NotFound($"No timelines found for conference ID: {conferenceId}");
            }

            // Chuyển đổi từ entity sang DTO để trả về
            var responseDtos = timeLines.Select(tl => new TimelineResponseDto
            {
                TimeLineId = tl.TimeLineId,
                ConferenceId = tl.ConferenceId,
                Date = tl.Date,
                Description = tl.Description
            }).ToList();

            return Ok(responseDtos);
        }

        /// <summary>
        /// Tạo một Timeline mới và lên lịch nhắc nhở.
        /// </summary>
        /// <param name="createDto">Thông tin Timeline cần tạo.</param>
        /// <returns>Timeline đã được tạo.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TimelineResponseDto>> CreateTimeline([FromForm] TimeLineCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về lỗi validation từ DTO
            }

            // Chuyển đổi DTO sang Entity
            var newTimeLine = new TimeLine
            {
                ConferenceId = createDto.ConferenceId,
                Date = createDto.Date.ToUniversalTime(), // Luôn dùng UTC cho server
                Description = createDto.Description,
                HangfireJobId = null // Job ID sẽ được gán bởi TimeLineManager
            };

            var createdTimeLine = await _timeLineManager.CreateTimelineWithReminderAsync(newTimeLine);

            // Chuyển đổi Entity sang Response DTO trước khi trả về
            var responseDto = new TimelineResponseDto
            {
                TimeLineId = createdTimeLine.TimeLineId,
                ConferenceId = createdTimeLine.ConferenceId,
                Date = createdTimeLine.Date,
                Description = createdTimeLine.Description
            };

            // Sử dụng GetTimeLinesByConference cho CreatedAtAction
            return CreatedAtAction(nameof(GetTimeLinesByConference), new { conferenceId = responseDto.ConferenceId }, responseDto);
        }

        /// <summary>
        /// Cập nhật một Timeline hiện có và cập nhật nhắc nhở.
        /// </summary>
        /// <param name="id">ID của Timeline cần cập nhật.</param>
        /// <param name="updateDto">Thông tin cập nhật Timeline.</param>
        /// <returns>Không có nội dung nếu thành công.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTimeline(int id, [FromForm] TimeLineUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tạo entity tạm thời với dữ liệu từ DTO và ID từ URL
            var timelineToUpdate = new TimeLine
            {
                TimeLineId = id, // ID từ URL
                Date = updateDto.Date.ToUniversalTime(),
                Description = updateDto.Description
                // ConferenceId không được cập nhật qua UpdateDto này, nếu cần thì lấy từ existingTimeLine
            };

            var updatedTimelineEntity = await _timeLineManager.UpdateTimelineWithReminderAsync(id, timelineToUpdate);

            if (updatedTimelineEntity == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Xóa một Timeline và nhắc nhở liên quan.
        /// </summary>
        /// <param name="id">ID của Timeline cần xóa.</param>
        /// <returns>Không có nội dung nếu thành công.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTimeline(int id)
        {
            var result = await _timeLineManager.DeleteTimelineAndReminderAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
