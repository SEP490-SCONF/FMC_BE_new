// BussinessObject.Entity/Schedule.cs
using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int ConferenceId { get; set; }

    public int? PaperId { get; set; }

    public string? SessionTitle { get; set; }

    public string? Location { get; set; } // Địa điểm thuyết trình

    public int? PresenterId { get; set; }

    public DateTime? PresentationStartTime { get; set; } // Thời gian bắt đầu

    public DateTime? PresentationEndTime { get; set; } // Thời gian kết thúc

    public virtual Conference Conference { get; set; } = null!;

    public virtual Paper? Paper { get; set; }

    public virtual User? Presenter { get; set; }
}