using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Paper
{
    public int PaperId { get; set; }

    public int ConferenceId { get; set; }

    public string? Title { get; set; }

    public string? Abstract { get; set; }

    public string? Keywords { get; set; }

    public int? TopicId { get; set; }

    public string? FilePath { get; set; }

    public string? Status { get; set; }

    public DateTime? SubmitDate { get; set; }

    public bool? IsPublished { get; set; }

    public decimal? PublicationFee { get; set; }
    public bool? IsPresented { get; set; }
    public int? PaymentId { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual ICollection<PaperAuthor> PaperAuthors { get; set; } = new List<PaperAuthor>();

    public virtual ICollection<PaperRevision> PaperRevisions { get; set; } = new List<PaperRevision>();

    public virtual Payment? Payment { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ReviewerAssignment> ReviewerAssignments { get; set; } = new List<ReviewerAssignment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual Topic? Topic { get; set; }

    public virtual ICollection<Proceeding> Proceedings { get; set; } = new List<Proceeding>();
}
