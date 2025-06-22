using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class ConferenceRole
{
    public int ConferenceRoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<UserConferenceRole> UserConferenceRoles { get; set; } = new List<UserConferenceRole>();
}
