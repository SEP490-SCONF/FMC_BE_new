using BussinessObject.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [Route("odata/[controller]")]
    public class ODataConferencesController : ODataController
    {
        private readonly IConferenceRepository _conferenceRepository;

        public ODataConferencesController(IConferenceRepository conferenceRepository)
        {
            _conferenceRepository = conferenceRepository;
        }

        [EnableQuery]
        [HttpGet]
        public IQueryable<Conference> Get()
        {
            return _conferenceRepository.GetAllQueryable();
        }
    }
}