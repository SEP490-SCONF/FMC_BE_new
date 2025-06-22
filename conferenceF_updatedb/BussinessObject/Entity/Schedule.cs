using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int ConferenceId { get; set; }

    public int? PaperId { get; set; }

    public string? SessionTitle { get; set; }

    public int? PresenterId { get; set; }

    public DateTime? PresentationTime { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual Paper? Paper { get; set; }

    public virtual User? Presenter { get; set; }
}
