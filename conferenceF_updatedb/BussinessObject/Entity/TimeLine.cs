using System;

namespace BussinessObject.Entity;

public partial class TimeLine
{
    public int TimeLineId { get; set; }
    public int ConferenceId { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public string? HangfireJobId { get; set; }

    public virtual Conference Conference { get; set; } = null!;
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>(); // Thêm collection của Schedule

}