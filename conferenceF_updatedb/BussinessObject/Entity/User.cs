using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string? Name { get; set; }

    public string? AvatarUrl { get; set; }

    public int RoleId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<AnswerLike> AnswerLikes { get; set; } = new List<AnswerLike>();

    public virtual ICollection<AnswerQuestion> AnswerQuestions { get; set; } = new List<AnswerQuestion>();

    public virtual ICollection<Conference> Conferences { get; set; } = new List<Conference>();

    public virtual ICollection<ForumQuestion> ForumQuestions { get; set; } = new List<ForumQuestion>();

    public virtual ICollection<NotificationStatus> NotificationStatuses { get; set; } = new List<NotificationStatus>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PaperAuthor> PaperAuthors { get; set; } = new List<PaperAuthor>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Proceeding> Proceedings { get; set; } = new List<Proceeding>();

    public virtual ICollection<QuestionLike> QuestionLikes { get; set; } = new List<QuestionLike>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();

    public virtual ICollection<ReviewerAssignment> ReviewerAssignments { get; set; } = new List<ReviewerAssignment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<UserConferenceRole> UserConferenceRoles { get; set; } = new List<UserConferenceRole>();
}
