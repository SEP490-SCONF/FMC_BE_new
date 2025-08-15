using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.DTOs.CallForPapers;
using ConferenceFWebAPI.DTOs.Conferences;
using ConferenceFWebAPI.DTOs.ConferenceTopics;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.DTOs.Papers;
using ConferenceFWebAPI.DTOs.Proccedings;
using ConferenceFWebAPI.DTOs.ReviewComments;
using ConferenceFWebAPI.DTOs.ReviewerAssignments;
using ConferenceFWebAPI.DTOs.ReviewHightlights;
using ConferenceFWebAPI.DTOs.Reviews;
using ConferenceFWebAPI.DTOs.User;
using ConferenceFWebAPI.DTOs.UserProfile;
using ConferenceFWebAPI.DTOs.Payment;

namespace ConferenceFWebAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Paper, PaperResponseWT>()
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic.TopicName)) // Lấy tên topic
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PaperAuthors.FirstOrDefault().Author.Name)) // Lấy tên tác giả
                .ForMember(dest => dest.PaperRevisions, opt => opt.MapFrom(src => src.PaperRevisions)) // Ánh xạ PaperRevisions


                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    src.PaperAuthors.FirstOrDefault() != null
                        ? src.PaperAuthors.FirstOrDefault().Author.Name
                        : "Unknown"))
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic.TopicName))
                .ForMember(dest => dest.IsAssigned, opt => opt.MapFrom(src =>
                    src.ReviewerAssignments != null && src.ReviewerAssignments.Any()))
                .ForMember(dest => dest.AssignedReviewerName, opt => opt.MapFrom(src =>
                    src.ReviewerAssignments
                        .OrderByDescending(ra => ra.AssignedAt) // hoặc OrderByDescending(ra => ra.AssignmentId)
                        .Select(ra => ra.Reviewer)
                        .FirstOrDefault(r =>
                            r.UserConferenceRoles.Any(ucr => ucr.ConferenceRole.RoleName == "Reviewer")
                        ).Name
                ))
                .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src =>
                    src.ReviewerAssignments
                        .OrderByDescending(ra => ra.AssignedAt)
                        .Select(ra => (int?)ra.AssignmentId)
                        .FirstOrDefault()
                ));


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
            CreateMap<Paper, PaperResponseDto>()
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic.TopicName))
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.PaperAuthors));

            CreateMap<PaperAuthor, AuthorDto>()
    .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Author.Name))
    .ForMember(dest => dest.AuthorOrder, opt => opt.MapFrom(src => src.AuthorOrder));

            CreateMap<PaperRevisionUploadDto, PaperRevision>()
                           .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // FilePath sẽ được xử lý riêng bởi Azure Blob Service
                           .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status sẽ được gán trong controller
                           .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore()); // SubmittedAt sẽ được gán trong controller
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationDto, Notification>();



            CreateMap<PaperRevision, PaperRevisionResponseDto>();
            CreateMap<ConferenceFWebAPI.DTOs.Reviews.HighlightAreaDTO, BussinessObject.Entity.HighlightArea>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PaperRevision, PaperRevisionDTO>();

            CreateMap<AddPaperRevisionDTO, PaperRevision>();
            CreateMap<UpdatePaperRevisionDTO, PaperRevision>();
            CreateMap<Review, ReviewWithHighlightAndCommentDTO>()
            .ForMember(dest => dest.RevisionStatus, opt => opt.MapFrom(src => src.Revision.Status))
            .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => src.Revision.FilePath))

            .ForMember(dest => dest.Highlights, opt => opt.MapFrom(src => src.ReviewHighlights))
            //.ForMember(dest => dest.CommentsList, opt => opt.MapFrom(src => src.ReviewComments))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.ReviewComments))

            .ForMember(dest => dest.Paper, opt => opt.MapFrom(src => src.Paper));

            CreateMap<Review, ReviewDTO>();
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

            CreateMap<AddOrUpdateTopicDTO, Topic>()
    .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.TopicName));


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
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
            
            CreateMap<UpdateUserDTO, User>()
    .ForAllMembers(opts =>
        opts.Condition((src, dest, srcMember) =>
            srcMember != null && !(srcMember is string str && string.IsNullOrWhiteSpace(str))));

            CreateMap<ReviewHighlight, ReviewHightlightDTO>();
            CreateMap<UpdateReviewHightlightDTO, ReviewHighlight>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AddReviewHighlightWithCommentDTO, ReviewHighlight>();

            CreateMap<ReviewComment, ReviewCommentDTO>();
            CreateMap<ReviewComment, CommentsDTO>();
            CreateMap<ReviewHighlight, HighlightDTO>();


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

            CreateMap<User, UserInformationDTO>()
    .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
    .ForMember(dest => dest.CreatedAt,
        opt => opt.MapFrom(src => DateTime.SpecifyKind(src.CreatedAt ?? DateTime.MinValue, DateTimeKind.Unspecified)));

            CreateMap<Payment, PaymentDTO>();
            CreateMap<CreatePaymentDTO, Payment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PaperId, opt => opt.MapFrom(src => src.PaperId));
            CreateMap<UpdatePaymentDTO, Payment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Conference, ConferenceResponseDTO>()
    .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics));

            CreateMap<Proceeding, ProceedingResponseDto>()
    .ForMember(dest => dest.PublishedByName, opt => opt.MapFrom(src => src.PublishedByNavigation.Name));
            CreateMap<ProceedingCreateDto, Proceeding>().ForMember(dest => dest.FilePath, opt => opt.Ignore());

            CreateMap<CallForPaper, CallForPaperDto>();


        }
        
    }
    
}
