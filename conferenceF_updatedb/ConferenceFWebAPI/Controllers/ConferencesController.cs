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
using ConferenceFWebAPI.DTOs.Conferences;

namespace FMC_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferencesController : ControllerBase
    {
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ITopicRepository _topicRepository;

        public ConferencesController(IConferenceRepository conferenceRepository, IAzureBlobStorageService azureBlobStorageService,IMapper mapper, IConfiguration configuration,
                                     IUserRepository userRepository, IEmailService emailService, ITopicRepository topicRepository)
        {
            _conferenceRepository = conferenceRepository;
            _azureBlobStorageService = azureBlobStorageService;
            _mapper = mapper;
            _configuration = configuration;
            _userRepository = userRepository;
            _emailService = emailService;
            _topicRepository = topicRepository;
        }




        // GET: api/Conference
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConferenceResponseDTO>>> GetAll()
        {
            var conferences = await _conferenceRepository.GetAll();
            var conferenceDTOs = _mapper.Map<IEnumerable<ConferenceResponseDTO>>(conferences);
            return Ok(conferenceDTOs);

        }

        // GET: api/Conference/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConferenceResponseDTO>> GetById(int id)
        {
            var conference = await _conferenceRepository.GetById(id);
            if (conference == null)
            {
                return NotFound($"Conference with ID {id} not found.");
            }

            var conferenceDTO = _mapper.Map<ConferenceResponseDTO>(conference);
            return Ok(conferenceDTO);
        }

        [HttpGet("topics/{id}")] // Route mới để phân biệt
        public async Task<ActionResult<ConferenceResponseDTO>> GetConferenceHasTopicsById(int id)
        {
            var conference = await _conferenceRepository.GetById(id);
            if (conference == null)
            {
                return NotFound($"Conference details for ID {id} not found.");
            }

            var conferenceDTO = _mapper.Map<ConferenceResponseDTO>(conference);

            // Lấy và gán danh sách Topics liên quan đến hội thảo
            var topics = await _topicRepository.GetTopicsByConferenceIdAsync(conference.ConferenceId);
            // Đảm bảo mapping từ Topic sang TopicDto đã được cấu hình trong AutoMapper
            conferenceDTO.Topics = _mapper.Map<List<TopicDTO>>(topics.ToList());

            return Ok(conferenceDTO);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConference([FromForm] ConferenceDTO conferenceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string bannerUrl = null;

                if (conferenceDto.BannerImage != null && conferenceDto.BannerImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(conferenceDto.BannerImage.FileName)?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    {
                        return BadRequest("Invalid image file format. Only .jpg, .jpeg, .png, .gif are allowed.");
                    }

                    var bannerContainerName = _configuration.GetValue<string>("BlobContainers:Banners");
                    if (string.IsNullOrEmpty(bannerContainerName))
                    {
                        return StatusCode(500, "Banner storage container name is not configured.");
                    }

                    bannerUrl = await _azureBlobStorageService.UploadFileAsync(conferenceDto.BannerImage, bannerContainerName);
                }

                var conference = _mapper.Map<Conference>(conferenceDto);

                conference.BannerUrl = bannerUrl; 
                conference.CreatedAt = DateTime.UtcNow; 
                                                       
                await _conferenceRepository.Add(conference); 
                return Ok(new
                {
                    Message = "Conference created successfully.",
                    ConferenceId = conference.ConferenceId, 
                    BannerUrl = conference.BannerUrl
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
