using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.DTOs.ReviewerAssignments;
using ConferenceFWebAPI.DTOs.Reviews;
using ConferenceFWebAPI.DTOs.UserProfile;

namespace ConferenceFWebAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Conference, ConferenceDTO>();
            CreateMap<ConferenceDTO, Conference>();
            CreateMap<Topic, TopicDTO>();
            CreateMap<TopicDTO, Topic>();
            CreateMap<Paper, PaperResponseDto>(); // <-- Thêm dòng này
            CreateMap<PaperRevisionUploadDto, PaperRevision>()
                           .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // FilePath sẽ được xử lý riêng bởi Azure Blob Service
                           .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status sẽ được gán trong controller
                           .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore()); // SubmittedAt sẽ được gán trong controller

            // Mapping từ PaperRevision Entity sang PaperRevisionResponseDto
            CreateMap<PaperRevision, PaperRevisionResponseDto>();
            CreateMap<AddPaperRevisionDTO, PaperRevision>();
            CreateMap<UpdatePaperRevisionDTO, PaperRevision>();
            CreateMap<Review, ReviewDTO>();
            CreateMap<AddReviewDTO, Review>();
            CreateMap<UpdateReviewDTO, Review>();
            CreateMap<ReviewerAssignment, ReviewerAssignmentDTO>();
            CreateMap<AddReviewerAssignmentDTO, ReviewerAssignment>();
            CreateMap<UpdateReviewerAssignmentDTO, ReviewerAssignment>();
            CreateMap<User, UserProfile>();


            
        }
    }
}
