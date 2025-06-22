using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class ForumQuestion
{
    public int FqId { get; set; }

    public int AskBy { get; set; }

    public int ForumId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Question { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AnswerQuestion> AnswerQuestions { get; set; } = new List<AnswerQuestion>();

    public virtual User AskByNavigation { get; set; } = null!;

    public virtual Forum Forum { get; set; } = null!;

    public virtual ICollection<QuestionLike> QuestionLikes { get; set; } = new List<QuestionLike>();
}
