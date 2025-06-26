using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BussinessObject.Entity;

public partial class ConferenceFTestContext : DbContext
{
    public ConferenceFTestContext()
    {
    }

    public ConferenceFTestContext(DbContextOptions<ConferenceFTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnswerLike> AnswerLikes { get; set; }

    public virtual DbSet<AnswerQuestion> AnswerQuestions { get; set; }

    public virtual DbSet<CallForPaper> CallForPapers { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Conference> Conferences { get; set; }

    public virtual DbSet<ConferenceRole> ConferenceRoles { get; set; }

    public virtual DbSet<Forum> Forums { get; set; }

    public virtual DbSet<ForumQuestion> ForumQuestions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationStatus> NotificationStatuses { get; set; }

    public virtual DbSet<Paper> Papers { get; set; }

    public virtual DbSet<PaperAuthor> PaperAuthors { get; set; }

    public virtual DbSet<PaperRevision> PaperRevisions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Proceeding> Proceedings { get; set; }

    public virtual DbSet<QuestionLike> QuestionLikes { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewComment> ReviewComments { get; set; }

    public virtual DbSet<ReviewHighlight> ReviewHighlights { get; set; }

    public virtual DbSet<ReviewerAssignment> ReviewerAssignments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserConferenceRole> UserConferenceRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerLike>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__AnswerLi__A2922C1487348A21");

            entity.ToTable("AnswerLike");

            entity.HasIndex(e => new { e.AnswerId, e.LikedBy }, "UQ_AnswerLike").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Answer).WithMany(p => p.AnswerLikes)
                .HasForeignKey(d => d.AnswerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswerLik__Answe__7D439ABD");

            entity.HasOne(d => d.LikedByNavigation).WithMany(p => p.AnswerLikes)
                .HasForeignKey(d => d.LikedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswerLik__Liked__7E37BEF6");
        });
         modelBuilder.Entity<Role>().HasData(
        new Role { RoleId = 1, RoleName = "Admin" },
        new Role { RoleId = 2, RoleName = "Member" 
        });
        modelBuilder.Entity<AnswerQuestion>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__AnswerQu__D482500427DFBA36");

            entity.ToTable("AnswerQuestion");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AnswerByNavigation).WithMany(p => p.AnswerQuestions)
                .HasForeignKey(d => d.AnswerBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswerQue__Answe__71D1E811");

            entity.HasOne(d => d.Fq).WithMany(p => p.AnswerQuestions)
                .HasForeignKey(d => d.FqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswerQues__FqId__70DDC3D8");

            entity.HasOne(d => d.ParentAnswer).WithMany(p => p.InverseParentAnswer)
                .HasForeignKey(d => d.ParentAnswerId)
                .HasConstraintName("FK__AnswerQue__Paren__72C60C4A");
        });

        modelBuilder.Entity<CallForPaper>(entity =>
        {
            entity.HasKey(e => e.Cfpid).HasName("PK__CallForP__A88117A04A2B1181");

            entity.ToTable("CallForPaper");

            entity.Property(e => e.Cfpid).HasColumnName("CFPId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.TemplatePath).HasMaxLength(500);

            entity.HasOne(d => d.Conference).WithMany(p => p.CallForPapers)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CallForPa__Confe__44CA3770");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__Certific__BBF8A7C17F0C82A8");

            entity.ToTable("Certificate");

            entity.HasIndex(e => e.RegId, "UQ_Certificate_Reg").IsUnique();

            entity.Property(e => e.CertificateNumber).HasMaxLength(100);
            entity.Property(e => e.CertificateUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IssueDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Reg).WithOne(p => p.Certificate)
                .HasForeignKey<Certificate>(d => d.RegId)
                .HasConstraintName("FK_Certificate_Registration");
        });

        modelBuilder.Entity<Conference>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("PK__Conferen__4A95F57384BD1A7D");

            entity.ToTable("Conference");

            entity.Property(e => e.BannerUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CallForPaper).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conferenc__Creat__5441852A");

            entity.HasMany(d => d.Topics).WithMany(p => p.Conferences)
                .UsingEntity<Dictionary<string, object>>(
                    "ConferenceTopic",
                    r => r.HasOne<Topic>().WithMany()
                        .HasForeignKey("TopicId")
                        .HasConstraintName("FK_ConferenceTopic_Topic"),
                    l => l.HasOne<Conference>().WithMany()
                        .HasForeignKey("ConferenceId")
                        .HasConstraintName("FK_ConferenceTopic_Conference"),
                    j =>
                    {
                        j.HasKey("ConferenceId", "TopicId");
                        j.ToTable("ConferenceTopic");
                    });
        });

        modelBuilder.Entity<ConferenceRole>(entity =>
        {
            entity.HasKey(e => e.ConferenceRoleId).HasName("PK__Conferen__39CD6654490CDB3B");

            entity.ToTable("ConferenceRole");

            entity.HasIndex(e => e.RoleName, "UQ__Conferen__8A2B6160AF36A424").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Forum>(entity =>
        {
            entity.HasKey(e => e.ForumId).HasName("PK__Forum__E210AC6F56E794C4");

            entity.ToTable("Forum");

            entity.HasIndex(e => e.ConferenceId, "UQ__Forum__4A95F5720353CE66").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithOne(p => p.Forum)
                .HasForeignKey<Forum>(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Forum__Conferenc__5EBF139D");
        });

        modelBuilder.Entity<ForumQuestion>(entity =>
        {
            entity.HasKey(e => e.FqId).HasName("PK__ForumQue__3BB79C1856AEF2B6");

            entity.ToTable("ForumQuestion");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.AskByNavigation).WithMany(p => p.ForumQuestions)
                .HasForeignKey(d => d.AskBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumQues__AskBy__6C190EBB");

            entity.HasOne(d => d.Forum).WithMany(p => p.ForumQuestions)
                .HasForeignKey(d => d.ForumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumQues__Forum__6D0D32F4");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotiId).HasName("PK__Notifica__EDC08E92567D61C6");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RoleTarget).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__282DF8C2");
        });

        modelBuilder.Entity<NotificationStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC078A7F3875");

            entity.ToTable("NotificationStatus");

            entity.HasIndex(e => new { e.NotiId, e.UserId }, "UQ__Notifica__3CB80257E086A0F9").IsUnique();

            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.ReadAt).HasColumnType("datetime");

            entity.HasOne(d => d.Noti).WithMany(p => p.NotificationStatuses)
                .HasForeignKey(d => d.NotiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__NotiI__498EEC8D");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationStatuses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__4A8310C6");
        });

        modelBuilder.Entity<Paper>(entity =>
        {
            entity.HasKey(e => e.PaperId).HasName("PK__Paper__AB86120B3A537ACE");

            entity.ToTable("Paper");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.Keywords).HasMaxLength(255);
            entity.Property(e => e.PublicationFee).HasColumnType("money");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SubmitDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.Papers)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Paper__Conferenc__07C12930");

            entity.HasOne(d => d.Payment).WithMany(p => p.Papers)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK_Paper_Payment");

            entity.HasOne(d => d.Topic).WithMany(p => p.Papers)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK__Paper__TopicId__08B54D69");
        });

        modelBuilder.Entity<PaperAuthor>(entity =>
        {
            entity.HasKey(e => new { e.PaperId, e.AuthorId }).HasName("PK__PaperAut__FC8BBDC8BF65C695");

            entity.ToTable("PaperAuthor");

            entity.Property(e => e.AuthorOrder).HasDefaultValue(1);

            entity.HasOne(d => d.Author).WithMany(p => p.PaperAuthors)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaperAuth__Autho__160F4887");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperAuthors)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaperAuth__Paper__151B244E");
        });

        modelBuilder.Entity<PaperRevision>(entity =>
        {
            entity.HasKey(e => e.RevisionId).HasName("PK__PaperRev__B4B1E3D1D2D55D9F");

            entity.ToTable("PaperRevision");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperRevisions)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaperRevi__Paper__1EA48E88");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PayId).HasName("PK__Payment__EE8FCECFAA2919C2");

            entity.ToTable("Payment");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PayOsCheckoutUrl)
                .HasMaxLength(500)
                .HasColumnName("PayOS_CheckoutUrl");
            entity.Property(e => e.PayOsOrderCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PayOS_OrderCode");
            entity.Property(e => e.PayStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Purpose).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__Confere__0E6E26BF");

            entity.HasOne(d => d.Paper).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__Payment__PaperId__114A936A");

            entity.HasOne(d => d.Reg).WithMany(p => p.Payments)
                .HasForeignKey(d => d.RegId)
                .HasConstraintName("FK__Payment__RegId__0F624AF8");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__UserId__0D7A0286");
        });

        modelBuilder.Entity<Proceeding>(entity =>
        {
            entity.HasKey(e => e.ProceedingId).HasName("PK__Proceedi__9710D55B8BE3C9BB");

            entity.ToTable("Proceeding");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.PublishedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Conference).WithMany(p => p.Proceedings)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Proceedin__Confe__3B40CD36");

            entity.HasOne(d => d.PublishedByNavigation).WithMany(p => p.Proceedings)
                .HasForeignKey(d => d.PublishedBy)
                .HasConstraintName("FK__Proceedin__Publi__3E1D39E1");

            entity.HasMany(d => d.Papers).WithMany(p => p.Proceedings)
                .UsingEntity<Dictionary<string, object>>(
                    "ProceedingPaper",
                    r => r.HasOne<Paper>().WithMany()
                        .HasForeignKey("PaperId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Proceedin__Paper__41EDCAC5"),
                    l => l.HasOne<Proceeding>().WithMany()
                        .HasForeignKey("ProceedingId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Proceedin__Proce__40F9A68C"),
                    j =>
                    {
                        j.HasKey("ProceedingId", "PaperId").HasName("PK__Proceedi__3DA8B47BECBBC263");
                        j.ToTable("ProceedingPaper");
                    });
        });

        modelBuilder.Entity<QuestionLike>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__Question__A2922C14D192659E");

            entity.ToTable("QuestionLike");

            entity.HasIndex(e => new { e.FqId, e.LikedBy }, "UQ_QuestionLike").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Fq).WithMany(p => p.QuestionLikes)
                .HasForeignKey(d => d.FqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuestionLi__FqId__778AC167");

            entity.HasOne(d => d.LikedByNavigation).WithMany(p => p.QuestionLikes)
                .HasForeignKey(d => d.LikedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuestionL__Liked__787EE5A0");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegId).HasName("PK__Registra__2C6822F85C9220A9");

            entity.ToTable("Registration");

            entity.HasIndex(e => new { e.UserId, e.ConferenceId }, "UQ__Registra__3321931AD118AF4D").IsUnique();

            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Conference).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Confe__03F0984C");

            entity.HasOne(d => d.User).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__UserI__02FC7413");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__74BC79CEC6C0E2AC");

            entity.ToTable("Review");

            entity.Property(e => e.ReviewedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Paper).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__PaperId__22751F6C");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__Reviewer__236943A5");

            entity.HasOne(d => d.Revision).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.RevisionId)
                .HasConstraintName("FK__Review__Revision__245D67DE");
        });

        modelBuilder.Entity<ReviewComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__ReviewCo__C3B4DFCAACD3E71E");

            entity.ToTable("ReviewComment");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("new");

            entity.HasOne(d => d.Highlight).WithMany(p => p.ReviewComments)
                .HasForeignKey(d => d.HighlightId)
                .HasConstraintName("FK__ReviewCom__Highl__3493CFA7");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewComments)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewCom__Revie__3587F3E0");

            entity.HasOne(d => d.User).WithMany(p => p.ReviewComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewCom__UserI__367C1819");
        });

        modelBuilder.Entity<ReviewHighlight>(entity =>
        {
            entity.HasKey(e => e.HighlightId).HasName("PK__ReviewHi__B11CEDF058DDC475");

            entity.ToTable("ReviewHighlight");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewHighlights)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewHig__Revie__30C33EC3");
        });

        modelBuilder.Entity<ReviewerAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Reviewer__32499E7786E3322E");

            entity.ToTable("ReviewerAssignment");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Paper).WithMany(p => p.ReviewerAssignments)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewerA__Paper__19DFD96B");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewerAssignments)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewerA__Revie__1AD3FDA4");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A7AF0DC58");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B6160B9E4028C").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__9C8A5B49A7037950");

            entity.ToTable("Schedule");

            entity.Property(e => e.PresentationTime).HasColumnType("datetime");
            entity.Property(e => e.SessionTitle).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Schedule__Confer__2BFE89A6");

            entity.HasOne(d => d.Paper).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK__Schedule__PaperI__2CF2ADDF");

            entity.HasOne(d => d.Presenter).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.PresenterId)
                .HasConstraintName("FK__Schedule__Presen__2DE6D218");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__Topic__022E0F5D531B8A1C");

            entity.ToTable("Topic");

            entity.Property(e => e.Status).HasDefaultValue(false);
            entity.Property(e => e.TopicName).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C40D8F6FB");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534E776CB52").IsUnique();

            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.TokenExpiry).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleId__4D94879B");
        });


        modelBuilder.Entity<UserConferenceRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserConf__3214EC078FA27F08");

            entity.ToTable("UserConferenceRole");

            entity.HasIndex(e => new { e.UserId, e.ConferenceRoleId, e.ConferenceId }, "UQ__UserConf__365E8FDDF40C0F64").IsUnique();

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Conference).WithMany(p => p.UserConferenceRoles)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserConfe__Confe__68487DD7");

            entity.HasOne(d => d.ConferenceRole).WithMany(p => p.UserConferenceRoles)
                .HasForeignKey(d => d.ConferenceRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserConfe__Confe__6754599E");

            entity.HasOne(d => d.User).WithMany(p => p.UserConferenceRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserConfe__UserI__66603565");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
