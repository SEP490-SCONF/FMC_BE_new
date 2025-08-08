using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Certificates;

namespace ConferenceFWebAPI.MappingProfiles
{
    public class CertificateMappingProfile : Profile
    {
        public CertificateMappingProfile()
        {
            CreateMap<Certificate, CertificateDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Reg != null ? src.Reg.User.Name : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.Reg != null ? src.Reg.User.Email : null))
                .ForMember(dest => dest.ConferenceTitle, opt => opt.MapFrom(src => src.Reg != null ? src.Reg.Conference.Title : null))
                .ForMember(dest => dest.ConferenceRoleName, opt => opt.MapFrom(src => src.UserConferenceRole != null ? src.UserConferenceRole.ConferenceRole.RoleName : null));

            CreateMap<CertificateCreateDto, Certificate>()
                .ForMember(dest => dest.CertificateId, opt => opt.Ignore())
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CertificateNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CertificateUrl, opt => opt.Ignore());

            CreateMap<CertificateUpdateDto, Certificate>()
                .ForMember(dest => dest.RegId, opt => opt.Ignore())
                .ForMember(dest => dest.IssueDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CertificateNumber, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
