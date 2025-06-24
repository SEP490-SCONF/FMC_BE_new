using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;

namespace ConferenceFWebAPI.MappingProfiles
{
    public class PaperProfile : Profile
    {
        public PaperProfile()
        {
            // Cấu hình ánh xạ từ PaperUploadDto sang Paper
            CreateMap<PaperUploadDto, Paper>()
                .ForMember(dest => dest.PaperId, opt => opt.Ignore()) // PaperId sẽ được tạo bởi DB, không có trong DTO
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // FilePath được gán thủ công sau khi upload
                .ForMember(dest => dest.SubmitDate, opt => opt.Ignore()) // SubmitDate được gán thủ công
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status được gán thủ công
                .ForMember(dest => dest.IsPublished, opt => opt.Ignore()) // IsPublished được gán thủ công
                                                                          // Bỏ qua các thuộc tính navigation hoặc các thuộc tính không có trong DTO
                                                                          // Bạn cần thêm các dòng ForMember này cho TẤT CẢ các thuộc tính của Paper entity
                                                                          // mà không có trong PaperUploadDto hoặc không nên được ánh xạ tự động.
                                                                          // Điều này đặc biệt quan trọng với các Navigation Properties của Entity Framework.
                .ForMember(dest => dest.PublicationFee, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Conference, opt => opt.Ignore())
                .ForMember(dest => dest.PaperAuthors, opt => opt.Ignore())
                .ForMember(dest => dest.PaperRevisions, opt => opt.Ignore())
                .ForMember(dest => dest.Payment, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewerAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore())
                .ForMember(dest => dest.Topic, opt => opt.Ignore())
                .ForMember(dest => dest.Proceedings, opt => opt.Ignore());
                //.ForMember(dest => dest.PdfFile, opt => opt.Ignore()); // Thêm dòng này vì PdfFile là IFormFile, không phải thuộc tính của Paper
        }
    }
}
