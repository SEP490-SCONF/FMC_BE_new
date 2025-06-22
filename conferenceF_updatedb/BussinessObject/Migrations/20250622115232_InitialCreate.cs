using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConferenceRole",
                columns: table => new
                {
                    ConferenceRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Conferen__39CD6654490CDB3B", x => x.ConferenceRoleId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__8AFACE1A7AF0DC58", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    TopicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    TopicName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Topic__022E0F5D531B8A1C", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CC4C40D8F6FB", x => x.UserId);
                    table.ForeignKey(
                        name: "FK__User__RoleId__4D94879B",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "Conference",
                columns: table => new
                {
                    ConferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    BannerUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CallForPaper = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Conferen__4A95F57384BD1A7D", x => x.ConferenceId);
                    table.ForeignKey(
                        name: "FK__Conferenc__Creat__5441852A",
                        column: x => x.CreatedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RoleTarget = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__EDC08E92567D61C6", x => x.NotiId);
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__282DF8C2",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CallForPaper",
                columns: table => new
                {
                    CFPId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime", nullable: true),
                    TemplatePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CallForP__A88117A04A2B1181", x => x.CFPId);
                    table.ForeignKey(
                        name: "FK__CallForPa__Confe__44CA3770",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceTopic",
                columns: table => new
                {
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConferenceTopic", x => new { x.ConferenceId, x.TopicId });
                    table.ForeignKey(
                        name: "FK_ConferenceTopic_Conference",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConferenceTopic_Topic",
                        column: x => x.TopicId,
                        principalTable: "Topic",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forum",
                columns: table => new
                {
                    ForumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Forum__E210AC6F56E794C4", x => x.ForumId);
                    table.ForeignKey(
                        name: "FK__Forum__Conferenc__5EBF139D",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "Proceeding",
                columns: table => new
                {
                    ProceedingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    PublishedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Proceedi__9710D55B8BE3C9BB", x => x.ProceedingId);
                    table.ForeignKey(
                        name: "FK__Proceedin__Confe__3B40CD36",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK__Proceedin__Publi__3E1D39E1",
                        column: x => x.PublishedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Registration",
                columns: table => new
                {
                    RegId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Registra__2C6822F85C9220A9", x => x.RegId);
                    table.ForeignKey(
                        name: "FK__Registrat__Confe__03F0984C",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK__Registrat__UserI__02FC7413",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserConferenceRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConferenceRoleId = table.Column<int>(type: "int", nullable: false),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserConf__3214EC078FA27F08", x => x.Id);
                    table.ForeignKey(
                        name: "FK__UserConfe__Confe__6754599E",
                        column: x => x.ConferenceRoleId,
                        principalTable: "ConferenceRole",
                        principalColumn: "ConferenceRoleId");
                    table.ForeignKey(
                        name: "FK__UserConfe__Confe__68487DD7",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK__UserConfe__UserI__66603565",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotiId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__3214EC078A7F3875", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Notificat__NotiI__498EEC8D",
                        column: x => x.NotiId,
                        principalTable: "Notifications",
                        principalColumn: "NotiId");
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__4A8310C6",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ForumQuestion",
                columns: table => new
                {
                    FqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AskBy = table.Column<int>(type: "int", nullable: false),
                    ForumId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ForumQue__3BB79C1856AEF2B6", x => x.FqId);
                    table.ForeignKey(
                        name: "FK__ForumQues__AskBy__6C190EBB",
                        column: x => x.AskBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__ForumQues__Forum__6D0D32F4",
                        column: x => x.ForumId,
                        principalTable: "Forum",
                        principalColumn: "ForumId");
                });

            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    CertificateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegId = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    CertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Certific__BBF8A7C17F0C82A8", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK_Certificate_Registration",
                        column: x => x.RegId,
                        principalTable: "Registration",
                        principalColumn: "RegId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerQuestion",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FqId = table.Column<int>(type: "int", nullable: false),
                    AnswerBy = table.Column<int>(type: "int", nullable: false),
                    ParentAnswerId = table.Column<int>(type: "int", nullable: true),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AnswerQu__D482500427DFBA36", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK__AnswerQue__Answe__71D1E811",
                        column: x => x.AnswerBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__AnswerQue__Paren__72C60C4A",
                        column: x => x.ParentAnswerId,
                        principalTable: "AnswerQuestion",
                        principalColumn: "AnswerId");
                    table.ForeignKey(
                        name: "FK__AnswerQues__FqId__70DDC3D8",
                        column: x => x.FqId,
                        principalTable: "ForumQuestion",
                        principalColumn: "FqId");
                });

            migrationBuilder.CreateTable(
                name: "QuestionLike",
                columns: table => new
                {
                    LikeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FqId = table.Column<int>(type: "int", nullable: false),
                    LikedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__A2922C14D192659E", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK__QuestionL__Liked__787EE5A0",
                        column: x => x.LikedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__QuestionLi__FqId__778AC167",
                        column: x => x.FqId,
                        principalTable: "ForumQuestion",
                        principalColumn: "FqId");
                });

            migrationBuilder.CreateTable(
                name: "AnswerLike",
                columns: table => new
                {
                    LikeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnswerId = table.Column<int>(type: "int", nullable: false),
                    LikedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AnswerLi__A2922C1487348A21", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK__AnswerLik__Answe__7D439ABD",
                        column: x => x.AnswerId,
                        principalTable: "AnswerQuestion",
                        principalColumn: "AnswerId");
                    table.ForeignKey(
                        name: "FK__AnswerLik__Liked__7E37BEF6",
                        column: x => x.LikedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Paper",
                columns: table => new
                {
                    PaperId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Abstract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TopicId = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubmitDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsPublished = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    PublicationFee = table.Column<decimal>(type: "money", nullable: true),
                    PaymentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Paper__AB86120B3A537ACE", x => x.PaperId);
                    table.ForeignKey(
                        name: "FK__Paper__Conferenc__07C12930",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK__Paper__TopicId__08B54D69",
                        column: x => x.TopicId,
                        principalTable: "Topic",
                        principalColumn: "TopicId");
                });

            migrationBuilder.CreateTable(
                name: "PaperAuthor",
                columns: table => new
                {
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaperAut__FC8BBDC8BF65C695", x => new { x.PaperId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK__PaperAuth__Autho__160F4887",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__PaperAuth__Paper__151B244E",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                });

            migrationBuilder.CreateTable(
                name: "PaperRevision",
                columns: table => new
                {
                    RevisionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaperRev__B4B1E3D1D2D55D9F", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK__PaperRevi__Paper__1EA48E88",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    RegId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    Currency = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    PayStatus = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PayOS_OrderCode = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    PayOS_CheckoutUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    PaperId = table.Column<int>(type: "int", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payment__EE8FCECFAA2919C2", x => x.PayId);
                    table.ForeignKey(
                        name: "FK__Payment__Confere__0E6E26BF",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK__Payment__PaperId__114A936A",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK__Payment__RegId__0F624AF8",
                        column: x => x.RegId,
                        principalTable: "Registration",
                        principalColumn: "RegId");
                    table.ForeignKey(
                        name: "FK__Payment__UserId__0D7A0286",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ProceedingPaper",
                columns: table => new
                {
                    ProceedingId = table.Column<int>(type: "int", nullable: false),
                    PaperId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Proceedi__3DA8B47BECBBC263", x => new { x.ProceedingId, x.PaperId });
                    table.ForeignKey(
                        name: "FK__Proceedin__Paper__41EDCAC5",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK__Proceedin__Proce__40F9A68C",
                        column: x => x.ProceedingId,
                        principalTable: "Proceeding",
                        principalColumn: "ProceedingId");
                });

            migrationBuilder.CreateTable(
                name: "ReviewerAssignment",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reviewer__32499E7786E3322E", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK__ReviewerA__Paper__19DFD96B",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK__ReviewerA__Revie__1AD3FDA4",
                        column: x => x.ReviewerId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    PaperId = table.Column<int>(type: "int", nullable: true),
                    SessionTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PresenterId = table.Column<int>(type: "int", nullable: true),
                    PresentationTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schedule__9C8A5B49A7037950", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK__Schedule__Confer__2BFE89A6",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK__Schedule__PaperI__2CF2ADDF",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK__Schedule__Presen__2DE6D218",
                        column: x => x.PresenterId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    RevisionId = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Review__74BC79CEC6C0E2AC", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK__Review__PaperId__22751F6C",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK__Review__Reviewer__236943A5",
                        column: x => x.ReviewerId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Review__Revision__245D67DE",
                        column: x => x.RevisionId,
                        principalTable: "PaperRevision",
                        principalColumn: "RevisionId");
                });

            migrationBuilder.CreateTable(
                name: "ReviewHighlight",
                columns: table => new
                {
                    HighlightId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: true),
                    OffsetStart = table.Column<int>(type: "int", nullable: true),
                    OffsetEnd = table.Column<int>(type: "int", nullable: true),
                    TextHighlighted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReviewHi__B11CEDF058DDC475", x => x.HighlightId);
                    table.ForeignKey(
                        name: "FK__ReviewHig__Revie__30C33EC3",
                        column: x => x.ReviewId,
                        principalTable: "Review",
                        principalColumn: "ReviewId");
                });

            migrationBuilder.CreateTable(
                name: "ReviewComment",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HighlightId = table.Column<int>(type: "int", nullable: true),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "new"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReviewCo__C3B4DFCAACD3E71E", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK__ReviewCom__Highl__3493CFA7",
                        column: x => x.HighlightId,
                        principalTable: "ReviewHighlight",
                        principalColumn: "HighlightId");
                    table.ForeignKey(
                        name: "FK__ReviewCom__Revie__3587F3E0",
                        column: x => x.ReviewId,
                        principalTable: "Review",
                        principalColumn: "ReviewId");
                    table.ForeignKey(
                        name: "FK__ReviewCom__UserI__367C1819",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerLike_LikedBy",
                table: "AnswerLike",
                column: "LikedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_AnswerLike",
                table: "AnswerLike",
                columns: new[] { "AnswerId", "LikedBy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerQuestion_AnswerBy",
                table: "AnswerQuestion",
                column: "AnswerBy");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerQuestion_FqId",
                table: "AnswerQuestion",
                column: "FqId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerQuestion_ParentAnswerId",
                table: "AnswerQuestion",
                column: "ParentAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_CallForPaper_ConferenceId",
                table: "CallForPaper",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "UQ_Certificate_Reg",
                table: "Certificate",
                column: "RegId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conference_CreatedBy",
                table: "Conference",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__Conferen__8A2B6160AF36A424",
                table: "ConferenceRole",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceTopic_TopicId",
                table: "ConferenceTopic",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "UQ__Forum__4A95F5720353CE66",
                table: "Forum",
                column: "ConferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumQuestion_AskBy",
                table: "ForumQuestion",
                column: "AskBy");

            migrationBuilder.CreateIndex(
                name: "IX_ForumQuestion_ForumId",
                table: "ForumQuestion",
                column: "ForumId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationStatus_UserId",
                table: "NotificationStatus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Notifica__3CB80257E086A0F9",
                table: "NotificationStatus",
                columns: new[] { "NotiId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paper_ConferenceId",
                table: "Paper",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Paper_PaymentId",
                table: "Paper",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Paper_TopicId",
                table: "Paper",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperAuthor_AuthorId",
                table: "PaperAuthor",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperRevision_PaperId",
                table: "PaperRevision",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ConferenceId",
                table: "Payment",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaperId",
                table: "Payment",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_RegId",
                table: "Payment",
                column: "RegId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_UserId",
                table: "Payment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Proceeding_ConferenceId",
                table: "Proceeding",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Proceeding_PublishedBy",
                table: "Proceeding",
                column: "PublishedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProceedingPaper_PaperId",
                table: "ProceedingPaper",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionLike_LikedBy",
                table: "QuestionLike",
                column: "LikedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_QuestionLike",
                table: "QuestionLike",
                columns: new[] { "FqId", "LikedBy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ConferenceId",
                table: "Registration",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "UQ__Registra__3321931AD118AF4D",
                table: "Registration",
                columns: new[] { "UserId", "ConferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Review_PaperId",
                table: "Review",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_ReviewerId",
                table: "Review",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_RevisionId",
                table: "Review",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComment_HighlightId",
                table: "ReviewComment",
                column: "HighlightId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComment_ReviewId",
                table: "ReviewComment",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComment_UserId",
                table: "ReviewComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignment_PaperId",
                table: "ReviewerAssignment",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignment_ReviewerId",
                table: "ReviewerAssignment",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHighlight_ReviewId",
                table: "ReviewHighlight",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "UQ__Role__8A2B6160B9E4028C",
                table: "Role",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_ConferenceId",
                table: "Schedule",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_PaperId",
                table: "Schedule",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_PresenterId",
                table: "Schedule",
                column: "PresenterId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D10534E776CB52",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConferenceRole_ConferenceId",
                table: "UserConferenceRole",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConferenceRole_ConferenceRoleId",
                table: "UserConferenceRole",
                column: "ConferenceRoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__UserConf__365E8FDDF40C0F64",
                table: "UserConferenceRole",
                columns: new[] { "UserId", "ConferenceRoleId", "ConferenceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Paper_Payment",
                table: "Paper",
                column: "PaymentId",
                principalTable: "Payment",
                principalColumn: "PayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Conferenc__Creat__5441852A",
                table: "Conference");

            migrationBuilder.DropForeignKey(
                name: "FK__Payment__UserId__0D7A0286",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK__Registrat__UserI__02FC7413",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK__Paper__Conferenc__07C12930",
                table: "Paper");

            migrationBuilder.DropForeignKey(
                name: "FK__Payment__Confere__0E6E26BF",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK__Registrat__Confe__03F0984C",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK__Payment__RegId__0F624AF8",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK__Paper__TopicId__08B54D69",
                table: "Paper");

            migrationBuilder.DropForeignKey(
                name: "FK_Paper_Payment",
                table: "Paper");

            migrationBuilder.DropTable(
                name: "AnswerLike");

            migrationBuilder.DropTable(
                name: "CallForPaper");

            migrationBuilder.DropTable(
                name: "Certificate");

            migrationBuilder.DropTable(
                name: "ConferenceTopic");

            migrationBuilder.DropTable(
                name: "NotificationStatus");

            migrationBuilder.DropTable(
                name: "PaperAuthor");

            migrationBuilder.DropTable(
                name: "ProceedingPaper");

            migrationBuilder.DropTable(
                name: "QuestionLike");

            migrationBuilder.DropTable(
                name: "ReviewComment");

            migrationBuilder.DropTable(
                name: "ReviewerAssignment");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "UserConferenceRole");

            migrationBuilder.DropTable(
                name: "AnswerQuestion");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Proceeding");

            migrationBuilder.DropTable(
                name: "ReviewHighlight");

            migrationBuilder.DropTable(
                name: "ConferenceRole");

            migrationBuilder.DropTable(
                name: "ForumQuestion");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "Forum");

            migrationBuilder.DropTable(
                name: "PaperRevision");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Conference");

            migrationBuilder.DropTable(
                name: "Registration");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Paper");
        }
    }
}
