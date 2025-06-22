using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class AnswerLike
{
    public int LikeId { get; set; }

    public int AnswerId { get; set; }

    public int LikedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AnswerQuestion Answer { get; set; } = null!;

    public virtual User LikedByNavigation { get; set; } = null!;
}
