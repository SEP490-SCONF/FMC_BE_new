using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserConferenceRolesController : ControllerBase
    {
        private readonly IUserConferenceRoleRepository _repo;

        public UserConferenceRolesController(IUserConferenceRoleRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repo.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetByConferenceId(int conferenceId)
        {
            var result = await _repo.GetByConferenceIdAsync(conferenceId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserConferenceRoleCreateDto dto)
        {
            var entity = new UserConferenceRole
            {
                UserId = dto.UserId,
                ConferenceRoleId = dto.ConferenceRoleId,
                ConferenceId = dto.ConferenceId,
                AssignedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserConferenceRole role)
        {
            if (id != role.Id) return BadRequest("ID mismatch.");
            await _repo.UpdateAsync(role);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
