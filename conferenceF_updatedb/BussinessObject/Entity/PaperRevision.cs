using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class PaperRevision
{
    public int RevisionId { get; set; }

    public int PaperId { get; set; }

    public string? FilePath { get; set; }

    public string? Status { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? Comments { get; set; }

    public virtual Paper Paper { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
