using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class QuestionLike
{
    public int LikeId { get; set; }

    public int FqId { get; set; }

    public int LikedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ForumQuestion Fq { get; set; } = null!;

    public virtual User LikedByNavigation { get; set; } = null!;
}
