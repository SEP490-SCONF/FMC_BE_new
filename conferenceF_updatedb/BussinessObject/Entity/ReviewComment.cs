using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class ReviewComment
{
    public int CommentId { get; set; }

    public int? HighlightId { get; set; }

    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public string? CommentText { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ReviewHighlight? Highlight { get; set; }

    public virtual Review Review { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
