using Microsoft.EntityFrameworkCore;
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
using ConferenceFWebAPI.MappingProfile;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ConferenceFTestContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Cấu hình EmailSettings từ appsettings.json
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Đăng ký các service
builder.Services.AddScoped<IEmailService, EmailService>();
// DAO registrations
builder.Services.AddScoped<UserDAO>();
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

// Conference
builder.Services.AddScoped<IConferenceRepository, ConferenceRepository>();

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
builder.Services.AddSingleton(new PayOS("295a3346-3eeb-449c-bb7b-cdbf495577ec", "a5e3d88f-3ae6-4235-b30e-e81c2b3686a2", "2a895d2b7938d4880973602f579a44043a2bc63183aa80e685ace2e9164cab5f"));
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

//Storage Google Drive
builder.Services.AddSingleton<GoogleDriveService>();

builder.Services.AddAutoMapper(typeof(PaperProfile).Assembly);

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

app.Run();
