using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class PaperAuthor
{
    public int PaperId { get; set; }

    public int AuthorId { get; set; }

    public int AuthorOrder { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual Paper Paper { get; set; } = null!;
}
