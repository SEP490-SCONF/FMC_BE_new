using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.DTOs.ReviewerAssignments;
using ConferenceFWebAPI.DTOs.Reviews;

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
            CreateMap<PaperRevision, PaperRevisionDTO>();
            CreateMap<AddPaperRevisionDTO, PaperRevision>();
            CreateMap<UpdatePaperRevisionDTO, PaperRevision>();
            CreateMap<Review, ReviewDTO>();
            CreateMap<AddReviewDTO, Review>();
            CreateMap<UpdateReviewDTO, Review>();
            CreateMap<ReviewerAssignment, ReviewerAssignmentDTO>();
            CreateMap<AddReviewerAssignmentDTO, ReviewerAssignment>();
            CreateMap<UpdateReviewerAssignmentDTO, ReviewerAssignment>();
        }
    }
}
