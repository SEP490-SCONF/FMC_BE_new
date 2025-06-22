using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Forum
{
    public int ForumId { get; set; }

    public int ConferenceId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual ICollection<ForumQuestion> ForumQuestions { get; set; } = new List<ForumQuestion>();
}
