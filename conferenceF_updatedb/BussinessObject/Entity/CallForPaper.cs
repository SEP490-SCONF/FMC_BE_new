using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class CallForPaper
{
    public int Cfpid { get; set; }

    public int ConferenceId { get; set; }

    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    public string? TemplatePath { get; set; }

    public bool Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Conference Conference { get; set; } = null!;
}
