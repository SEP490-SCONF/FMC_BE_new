﻿using Microsoft.EntityFrameworkCore;
using Repository;
using Microsoft.AspNetCore.OData;
using Net.payOS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DataAccess;



using BussinessObject.Entity;
using ConferenceFWebAPI.Service;
using Repository.Repository;
using ConferenceFWebAPI.MappingProfiles;
using System.Text.Json.Serialization;
using ConferenceFWebAPI.Hubs;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);
// 1. Lấy chuỗi kết nối SignalR từ appsettings.json
var signalRConnectionString = builder.Configuration.GetConnectionString("AzureSignalR");
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Paper>("Papers"); // Register your Paper entity as an OData EntitySet
modelBuilder.EntitySet<Review>("Reviews");

builder.Services.AddControllers().AddOData(
    options => options.Select() // Enable $select
                      .Expand() // Enable $expand
                      .Filter() // Enable $filter
                      .OrderBy() // Enable $orderby
                      .SetMaxTop(100) // Set max $top value (e.g., limit results to 100)
    .Count() // Enable $count
                      .AddRouteComponents("odata", modelBuilder.GetEdmModel()) // Define the OData route
);

// 2. Thêm dịch vụ SignalR và kết nối với Azure SignalR Service
builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);

builder.Services.AddDbContext<ConferenceFTestContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Cấu hình EmailSettings từ appsettings.json
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Đăng ký các service
builder.Services.AddScoped<IEmailService, EmailService>();
// DAO registrations
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<RoleDAO>();
builder.Services.AddScoped<ConferenceRoleDAO>();
builder.Services.AddScoped<AnswerLikeDAO>();
builder.Services.AddScoped<AnswerQuestionDAO>();
builder.Services.AddScoped<CallForPaperDAO>();
builder.Services.AddScoped<ConferenceDAO>();
builder.Services.AddScoped<ForumDAO>();
builder.Services.AddScoped<ForumQuestionDAO>();
builder.Services.AddScoped<NotificationDAO>();
builder.Services.AddScoped<PaperDAO>();
builder.Services.AddScoped<PaperRevisionDAO>();
builder.Services.AddScoped<PaymentDAO>();
builder.Services.AddScoped<ProceedingDAO>();
builder.Services.AddScoped<QuestionLikeDAO>();
builder.Services.AddScoped<RegistrationDAO>();
builder.Services.AddScoped<ReviewDAO>();
builder.Services.AddScoped<ReviewCommentDAO>();
builder.Services.AddScoped<ReviewHighlightDAO>();
builder.Services.AddScoped<ReviewerAssignmentDAO>();
builder.Services.AddScoped<ScheduleDAO>();
builder.Services.AddScoped<TopicDAO>();
builder.Services.AddScoped<UserConferenceRoleDAO>();

// Add Scoped services for each repository
// User
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Role
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
// Conference
builder.Services.AddScoped<IConferenceRepository, ConferenceRepository>();
//
builder.Services.AddScoped<IConferenceRoleRepository, ConferenceRoleRepository>();

// Topic
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

// Paper
builder.Services.AddScoped<IPaperRepository, PaperRepository>();

// Review
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Registration
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// Forum
builder.Services.AddScoped<IForumRepository, ForumRepository>();

// Notification
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Payment
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Proceeding
builder.Services.AddScoped<IProceedingRepository, ProceedingRepository>();

// PaperRevision
builder.Services.AddScoped<IPaperRevisionRepository, PaperRevisionRepository>();

// ReviewComment
builder.Services.AddScoped<IReviewCommentRepository, ReviewCommentRepository>();

// AnswerQuestion
builder.Services.AddScoped<IAnswerQuestionRepository, AnswerQuestionRepository>();

// ForumQuestion
builder.Services.AddScoped<IForumQuestionRepository, ForumQuestionRepository>();

// ReviewHighlight
builder.Services.AddScoped<IReviewHighlightRepository, ReviewHighlightRepository>();

// AnswerLike
builder.Services.AddScoped<IAnswerLikeRepository, AnswerLikeRepository>();

// QuestionLike
builder.Services.AddScoped<IQuestionLikeRepository, QuestionLikeRepository>();

// CallForPaper
builder.Services.AddScoped<ICallForPaperRepository, CallForPaperRepository>();

// Schedule
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

// ReviewerAssignment
builder.Services.AddScoped<IReviewerAssignmentRepository, ReviewerAssignmentRepository>();

// UserConferenceRole
builder.Services.AddScoped<IUserConferenceRoleRepository, UserConferenceRoleRepository>();

//PAYOS
builder.Services.AddSingleton(new PayOS("acb29369-96a5-4ec4-bd78-b8fa316bfee6", "41751deb-82ed-4c9b-80a9-3d6cdb1cd940", "e4850839d6415b5a8e7fd60d16271e14b0adc351c7a977a40548649186da072f"));
//AddCors
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpecificOrigin", build =>
    {
        build.WithOrigins("http://localhost:5173")
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials();
    });
});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("SpecificOrigin", build =>
//    {
//        build.WithOrigins("http://localhost:5174")
//             .AllowAnyMethod()
//             .AllowAnyHeader()
//             .AllowCredentials();
//    });
//});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;

    // Cấu hình định dạng DateTime
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    });

//AddAuthentication

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddAutoMapper(typeof(PaperProfile).Assembly);

builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();

// Add services to the container.
builder.Services.AddControllers().AddOData(opt => opt.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Đảm bảo dòng này được gọi
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("SpecificOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// 3. Map Hub của bạn tới một endpoint
app.MapHub<NotificationHub>("/notificationhub"); // Client sẽ kết nối tới URL này

app.Run();
