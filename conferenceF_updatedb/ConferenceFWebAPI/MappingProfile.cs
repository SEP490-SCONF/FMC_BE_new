using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.DTOs.Conferences;
using ConferenceFWebAPI.DTOs.ConferenceTopics;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.DTOs.ReviewComments;
using ConferenceFWebAPI.DTOs.ReviewerAssignments;
using ConferenceFWebAPI.DTOs.ReviewHightlights;
using ConferenceFWebAPI.DTOs.Reviews;
using ConferenceFWebAPI.DTOs.User;
using ConferenceFWebAPI.DTOs.UserProfile;

namespace ConferenceFWebAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserInfomation>();
            CreateMap<User, UserDto>();
            CreateMap<Conference, ConferenceDTO>();
            CreateMap<ConferenceDTO, Conference>();
            CreateMap<Conference, ConferenceResponseDTO>();
            CreateMap<ConferenceResponseDTO, Conference>();
            CreateMap<ConferenceUpdateDTO, Conference>()
           .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Topic, TopicDTO>();
            CreateMap<TopicDTO, Topic>();
            CreateMap<Paper, PaperResponseDto>(); // <-- Thêm dòng này
            CreateMap<PaperRevisionUploadDto, PaperRevision>()
                           .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // FilePath sẽ được xử lý riêng bởi Azure Blob Service
                           .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status sẽ được gán trong controller
                           .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore()); // SubmittedAt sẽ được gán trong controller

            // Mapping từ PaperRevision Entity sang PaperRevisionResponseDto
            CreateMap<PaperRevision, PaperRevisionResponseDto>();
            CreateMap<PaperRevision, PaperRevisionDTO>();

            CreateMap<AddPaperRevisionDTO, PaperRevision>();
            CreateMap<UpdatePaperRevisionDTO, PaperRevision>();
            CreateMap<Review, ReviewWithHighlightAndCommentDTO>()
            .ForMember(dest => dest.RevisionStatus, opt => opt.MapFrom(src => src.Revision.Status))
            .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => src.Revision.FilePath))

            .ForMember(dest => dest.Highlights, opt => opt.MapFrom(src => src.ReviewHighlights))
            .ForMember(dest => dest.CommentsList, opt => opt.MapFrom(src => src.ReviewComments))
            .ForMember(dest => dest.Paper, opt => opt.MapFrom(src => src.Paper)); 


            CreateMap<AddReviewDTO, Review>();
            CreateMap<AddReviewWithHighlightAndCommentDTO, Review>();

            CreateMap<UpdateReviewDTO, Review>().
                ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateReviewWithHighlightAndCommentDTO, Review>().
               ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateReviewWithHighlightAndCommentDTO, ReviewHighlight>()
            .ForAllMembers(opts =>
            opts.Condition((src, dest, srcMember) =>
            srcMember != null && !(srcMember is string str && string.IsNullOrWhiteSpace(str)))
             );

            CreateMap<UpdateReviewWithHighlightAndCommentDTO, ReviewComment>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) =>
                        srcMember != null && !(srcMember is string str && string.IsNullOrWhiteSpace(str))
                    )
                );



            CreateMap<ReviewerAssignment, ReviewerAssignmentDTO>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Paper.Title))
                .ForMember(dest => dest.Abstract, opt => opt.MapFrom(src => src.Paper.Abstract))
                .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Paper.Keywords))
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Paper.TopicId))
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Paper.Topic.TopicName)) 

                .ForMember(dest => dest.Revisions,
                 opt => opt.MapFrom(src => src.Paper.PaperRevisions
                                           .Where(r => r.Status == "Under Review")))
;


            CreateMap<AddReviewerAssignmentDTO, ReviewerAssignment>();
            CreateMap<UpdateReviewerAssignmentDTO, ReviewerAssignment>();
            CreateMap<User, UserProfile>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.CreatedAt,
        opt => opt.MapFrom(src => DateTime.SpecifyKind(src.CreatedAt ?? DateTime.MinValue, DateTimeKind.Unspecified)));;
            CreateMap<ReviewHighlight, ReviewHightlightDTO>();
            CreateMap<UpdateReviewHightlightDTO, ReviewHighlight>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AddReviewHighlightWithCommentDTO, ReviewHighlight>();

            CreateMap<ReviewComment, ReviewCommentDTO>();
            CreateMap<AddReviewCommentDTO, ReviewComment>();
            CreateMap<UpdateReviewCommentDTO, ReviewComment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ConferenceTopicDTO, Dictionary<string, object>>()
    .ConvertUsing(dto => new Dictionary<string, object>
    {
        { "ConferenceId", dto.ConferenceId },
        { "TopicId", dto.TopicId }
    });

            CreateMap<Dictionary<string, object>, ConferenceTopicDTO>()
                .ConvertUsing(dict => new ConferenceTopicDTO
                {
                    ConferenceId = (int)dict["ConferenceId"],
                    TopicId = (int)dict["TopicId"]
                });





        }
    }
}
