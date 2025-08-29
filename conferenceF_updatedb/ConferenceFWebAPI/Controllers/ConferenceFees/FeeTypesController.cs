using AutoMapper;
using ConferenceFWebAPI.DTOs.ConferenceFees;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.ConferenceFees
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeeTypesController : ControllerBase
    {
        private readonly IFeeTypeRepository _feeTypeRepository;
        private readonly IMapper _mapper;

        public FeeTypesController(IFeeTypeRepository feeTypeRepository, IMapper mapper)
        {
            _feeTypeRepository = feeTypeRepository;
            _mapper = mapper;
        }

        // GET: /api/feetypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeeTypeDto>>> GetAll()
        {
            var feeTypes = await _feeTypeRepository.GetAll();
            var result = feeTypes.Select(ft => _mapper.Map<FeeTypeDto>(ft));
            return Ok(result);
        }
    }
}
