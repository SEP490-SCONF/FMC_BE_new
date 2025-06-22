using AutoMapper;
using BussinessObject.Entity;

namespace ConferenceFWebAPI.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PaperCreateDto, Paper>();
            CreateMap<Paper, PaperDto>();

        }
    }
}
