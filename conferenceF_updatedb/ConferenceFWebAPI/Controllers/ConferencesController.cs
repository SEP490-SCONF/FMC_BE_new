using Microsoft.AspNetCore.Mvc;
using Repository;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using AutoMapper;
using AutoMapper.Internal.Mappers;
using ConferenceFWebAPI.Service;
using Google.Apis.Drive.v3.Data;
using DataAccess;
using ConferenceFWebAPI.DTOs.Paper;

namespace FMC_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferencesController : ControllerBase
    {
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public ConferencesController(IConferenceRepository conferenceRepository, IMapper mapper,
                                     IUserRepository userRepository, IEmailService emailService)
        {
            _conferenceRepository = conferenceRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _emailService = emailService;
        }




        // GET: api/Conference
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConferenceDTO>>> GetAll()
        {
            var conferences = await _conferenceRepository.GetAll();
            var conferenceDTOs = _mapper.Map<IEnumerable<ConferenceDTO>>(conferences);
            return Ok(conferenceDTOs);

        }

        // GET: api/Conference/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConferenceDTO>> GetById(int id)
        {
            var conference = await _conferenceRepository.GetById(id);
            if (conference == null)
            {
                return NotFound($"Conference with ID {id} not found.");
            }

            var conferenceDTO = _mapper.Map<ConferenceDTO>(conference);
            return Ok(conferenceDTO);
        }


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ConferenceDTO conferenceDto)
        {
            if (conferenceDto == null)
                return BadRequest("Conference data is null.");
            var user = await _userRepository.GetById(conferenceDto.CreatedBy);
            if (user == null)
                return BadRequest("CreatedBy not found");
                var conference = _mapper.Map<Conference>(conferenceDto);
            conference.CreatedAt = DateTime.UtcNow.AddHours(7);
            conference.Status = true;
           
            await _conferenceRepository.Add(conference);

            // Gửi email cho Organizer
            //var organizers = await _userRepository.GetOrganizers();
            //var emailBody = ConferenceCreatedTemplate.GetHtml(conference);

            //foreach (var organizer in organizers)
            //{
            //    await _emailService.SendEmailAsync(organizer.Email,
            //        $"[Thông báo] Hội thảo mới: {conference.Title}",
            //        emailBody);
            //}

            return CreatedAtAction(nameof(GetById), new { id = conference.ConferenceId }, conferenceDto);
        }


        // PUT: api/Conference/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ConferenceDTO conferenceDTO)
        {
            if (id == 0 )
            {
                return BadRequest("ID is requied");
            }

            try
            {
                if (conferenceDTO.CreatedBy == 0) return BadRequest("CreateBy is requied");
                var user = await _userRepository.GetById(conferenceDTO.CreatedBy);
                if (user == null)
                    return BadRequest("CreatedBy not found");
                var con = await _conferenceRepository.GetById(id);
                if (user == null)
                    return BadRequest("Conference not found");
                _mapper.Map(conferenceDTO,con );
                
                await _conferenceRepository.Update(con);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpPost("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id)
        {
            try
            {
                var existingConference = await _conferenceRepository.GetById(id);
                if (existingConference == null)
                {
                    return NotFound($"Conference with ID {id} not found.");
                }
                await _conferenceRepository.UpdateConferenceStatus(id, true);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating status: {ex.Message}");
            }
        }

        // GET: api/Conference/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount()
        {
            var count = await _conferenceRepository.GetConferenceCount();
            return Ok(count);
        }

    }
}
