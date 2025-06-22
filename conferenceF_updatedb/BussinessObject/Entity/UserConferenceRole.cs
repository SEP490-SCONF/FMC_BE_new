using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class UserConferenceRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ConferenceRoleId { get; set; }

    public int ConferenceId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual ConferenceRole ConferenceRole { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
