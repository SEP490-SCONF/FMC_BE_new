using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.PaperRevisions
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaperRevisionController : ControllerBase
    {
        private readonly IPaperRevisionRepository _revisionRepository;
        private readonly IMapper _mapper;

        public PaperRevisionController(IPaperRevisionRepository revisionRepository, IMapper mapper)
        {
            _revisionRepository = revisionRepository;
            _mapper = mapper;
        }

        // GET: api/PaperRevision
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var revisions = await _revisionRepository.GetAll();
            var result = _mapper.Map<IEnumerable<PaperRevisionDTO>>(revisions);
            return Ok(result);
        }

        // GET: api/PaperRevision/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var revision = await _revisionRepository.GetById(id);
            if (revision == null)
                return NotFound($"Revision with ID {id} not found.");
            return Ok(_mapper.Map<PaperRevisionDTO>(revision));
        }

        // GET: api/PaperRevision/paper/{paperId}
        [HttpGet("paper/{paperId}")]
        public async Task<IActionResult> GetByPaperId(int paperId)
        {
            var revisions = await _revisionRepository.GetByPaperId(paperId);
            var result = _mapper.Map<IEnumerable<PaperRevisionDTO>>(revisions);
            return Ok(result);
        }

        // POST: api/PaperRevision
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddPaperRevisionDTO dto)
        {
            var revision = _mapper.Map<PaperRevision>(dto);
            await _revisionRepository.Add(revision);
            return CreatedAtAction(nameof(GetById), new { id = revision.RevisionId }, _mapper.Map<PaperRevisionDTO>(revision));
        }

        // PUT: api/PaperRevision/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePaperRevisionDTO dto)
        {
            var existing = await _revisionRepository.GetById(id);
            if (existing == null)
                return NotFound($"Revision with ID {id} not found.");

            _mapper.Map(dto, existing);
            await _revisionRepository.Update(existing);
            return NoContent();
        }

        // DELETE: api/PaperRevision/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _revisionRepository.GetById(id);
            if (existing == null)
                return NotFound($"Revision with ID {id} not found.");

            await _revisionRepository.Delete(id);
            return NoContent();
        }
    }
}
