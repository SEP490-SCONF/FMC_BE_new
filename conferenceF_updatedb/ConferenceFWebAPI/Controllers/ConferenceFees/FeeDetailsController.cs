using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.ConferenceFees;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.ConferenceFees
{
    [ApiController]
    public class FeeDetailsController : ControllerBase
    {
        private readonly IFeeDetailRepository _feeDetailRepository;
        private readonly IMapper _mapper;

        public FeeDetailsController(IFeeDetailRepository feeDetailRepository, IMapper mapper)
        {
            _feeDetailRepository = feeDetailRepository;
            _mapper = mapper;
        }

        // GET: /api/conferences/{conferenceId}/fees
        [HttpGet("api/conferences/{conferenceId}/fees")]
        public async Task<ActionResult<IEnumerable<FeeDetailPublicDto>>> GetByConferenceId(int conferenceId)
        {
            var fees = await _feeDetailRepository.GetByConferenceId(conferenceId);
            var result = fees.Select(f => _mapper.Map<FeeDetailPublicDto>(f));
            return Ok(result);
        }

        // GET: /api/fees/{feeDetailId}
        [HttpGet("api/fees/{feeDetailId}")]
        public async Task<ActionResult<FeeDetailOrganizerDto>> GetById(int feeDetailId)
        {
            var fee = await _feeDetailRepository.GetById(feeDetailId);
            if (fee == null)
                return NotFound();
            var result = _mapper.Map<FeeDetailOrganizerDto>(fee);
            return Ok(result);
        }

        // POST: /api/conferences/{conferenceId}/fees
        [HttpPost("api/conferences/{conferenceId}/fees")]
        public async Task<ActionResult> Create(int conferenceId, [FromBody] FeeDetailCreateDto dto)
        {
            var feeDetail = _mapper.Map<FeeDetail>(dto);
            feeDetail.ConferenceId = conferenceId;
            await _feeDetailRepository.Add(feeDetail);
            return Ok(new { Message = "FeeDetail created successfully." });
        }

        // PUT: /api/fees/{feeDetailId}
        [HttpPut("api/fees/{feeDetailId}")]
        public async Task<ActionResult> Update(int feeDetailId, [FromBody] FeeDetailUpdateDto dto)
        {
            var existing = await _feeDetailRepository.GetById(feeDetailId);
            if (existing == null)
                return NotFound();

            existing.Amount = dto.Amount;
            existing.Mode = dto.Mode;
            existing.Note = dto.Note;
            existing.IsVisible = dto.IsVisible;

            await _feeDetailRepository.Update(existing);
            return Ok(new { Message = "FeeDetail updated successfully." });
        }
    }
}
