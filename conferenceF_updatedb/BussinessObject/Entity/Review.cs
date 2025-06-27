using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? PaperId { get; set; }

    public int ReviewerId { get; set; }

    public int RevisionId { get; set; }

    public int? Score { get; set; }

    public string? Comments { get; set; }

    public string? Status { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Paper Paper { get; set; } = null!;

    public virtual ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();

    public virtual ICollection<ReviewHighlight> ReviewHighlights { get; set; } = new List<ReviewHighlight>();

    public virtual User Reviewer { get; set; } = null!;

    public virtual PaperRevision? Revision { get; set; }
}
