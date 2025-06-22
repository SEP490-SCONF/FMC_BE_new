using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class AnswerQuestion
{
    public int AnswerId { get; set; }

    public int FqId { get; set; }

    public int AnswerBy { get; set; }

    public int? ParentAnswerId { get; set; }

    public string Answer { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User AnswerByNavigation { get; set; } = null!;

    public virtual ICollection<AnswerLike> AnswerLikes { get; set; } = new List<AnswerLike>();

    public virtual ForumQuestion Fq { get; set; } = null!;

    public virtual ICollection<AnswerQuestion> InverseParentAnswer { get; set; } = new List<AnswerQuestion>();

    public virtual AnswerQuestion? ParentAnswer { get; set; }
}
