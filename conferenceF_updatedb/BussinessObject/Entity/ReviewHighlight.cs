using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class ReviewHighlight
{
    public int HighlightId { get; set; }

    public int ReviewId { get; set; }

    public int? PageNumber { get; set; }

    public int? OffsetStart { get; set; }

    public int? OffsetEnd { get; set; }

    public string? TextHighlighted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Review Review { get; set; } = null!;

    public virtual ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}
