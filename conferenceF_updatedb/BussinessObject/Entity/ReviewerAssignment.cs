using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class ReviewerAssignment
{
    public int AssignmentId { get; set; }

    public int PaperId { get; set; }

    public int ReviewerId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Paper Paper { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
