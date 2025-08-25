using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.DTOs.CallForPapers;
using ConferenceFWebAPI.DTOs.Conferences;
using ConferenceFWebAPI.DTOs.ConferenceTopics;
using ConferenceFWebAPI.DTOs.Papers;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.DTOs.Proccedings;
using ConferenceFWebAPI.DTOs.ReviewComments;
using ConferenceFWebAPI.DTOs.ReviewerAssignments;
using ConferenceFWebAPI.DTOs.ReviewHightlights;
using ConferenceFWebAPI.DTOs.Reviews;
using ConferenceFWebAPI.DTOs.User;
using ConferenceFWebAPI.DTOs.UserProfile;
using ConferenceFWebAPI.DTOs.Payment;
using ConferenceFWebAPI.DTOs.Schedules;

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
                )).ForMember(dest => dest.PaperScore, opt => opt.MapFrom(src =>
        src.PaperRevisions
            .Where(pr => pr.Status == "Accepted")
            .SelectMany(pr => pr.Reviews)
            .Select(r => r.Score ?? 0)
            .DefaultIfEmpty(0)
            .Average() != null
                ? (int?)Math.Round(
                    src.PaperRevisions
                        .Where(pr => pr.Status == "Accepted")
                        .SelectMany(pr => pr.Reviews)
                        .Select(r => r.Score ?? 0)
                        .DefaultIfEmpty(0)
                        .Average()
                  )
                : null
    ));


            CreateMap<Schedule, ScheduleRequestDto>();
            CreateMap<ScheduleRequestDto, Schedule>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Schedule, ScheduleUpdateDto>();
            CreateMap<ScheduleUpdateDto, Schedule>()
    .ForMember(dest => dest.PaperId, opt => opt.MapFrom(src => src.PaperId))
    .ForMember(dest => dest.PresenterId, opt => opt.MapFrom(src => src.PresenterId))
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember, context) =>
    {
        // Nếu là PaperId hoặc PresenterId thì cho map luôn (kể cả null)
        if (opts.DestinationMember.Name == nameof(Schedule.PaperId) ||
            opts.DestinationMember.Name == nameof(Schedule.PresenterId))
            return true;

        // Các field khác: chỉ map khi srcMember != null
        return srcMember != null;
    }));


            CreateMap<Schedule, ScheduleResponseDto>()
                .ForMember(dest => dest.PaperScore, opt => opt.MapFrom(src =>
                    src.Paper != null
                    ? (int?)Math.Round(
                        src.Paper.PaperRevisions
                            .Where(pr => pr.Status == "Accepted")
                            .SelectMany(pr => pr.Reviews)
                            .Select(r => r.Score ?? 0)
                            .DefaultIfEmpty(0)
                            .Average()
                      )
                    : null
                ));
            CreateMap<ScheduleResponseDto, Schedule>();




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

            CreateMap<Review, ReviewDTO>()
                .ForMember(dest => dest.PaperTitle, opt => opt.MapFrom(src => src.Paper.Title))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                    src.Paper.PaperAuthors.FirstOrDefault() != null
                    ? src.Paper.PaperAuthors.FirstOrDefault().Author.Name
                    : "Unknown"
                ));
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
                                          ))
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

            CreateMap<Payment, PaymentDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User == null ? null : src.User.Name))
                .ForMember(dest => dest.ConferenceName, opt => opt.MapFrom(src => src.Conference == null ? null : src.Conference.Title));
            CreateMap<CreatePaymentDTO, Payment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PaperId, opt => opt.MapFrom(src => src.PaperId));
            CreateMap<UpdatePaymentDTO, Payment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Conference, ConferenceResponseDTO>()
                .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics));

            CreateMap<Proceeding, ProceedingResponseDto>();
            CreateMap<ProceedingCreateDto, Proceeding>().ForMember(dest => dest.FilePath, opt => opt.Ignore());
            CreateMap<ProceedingUpdateFromFormDto, Proceeding>()
    .ForAllMembers(opts =>
        opts.Condition((src, dest, srcMember) =>
            srcMember != null &&
            (!(srcMember is string str) || !string.IsNullOrEmpty(str))
        )
    );


            CreateMap<CallForPaper, CallForPaperDto>();


        }
        
    }
    
}
