using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Conference
{
    public int ConferenceId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Location { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public string? BannerUrl { get; set; }

    public string? CallForPaper { get; set; }

    public virtual ICollection<CallForPaper> CallForPapers { get; set; } = new List<CallForPaper>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Forum? Forum { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Proceeding> Proceedings { get; set; } = new List<Proceeding>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<UserConferenceRole> UserConferenceRoles { get; set; } = new List<UserConferenceRole>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public virtual ICollection<TimeLine> TimeLines { get; set; } = new List<TimeLine>();

}
