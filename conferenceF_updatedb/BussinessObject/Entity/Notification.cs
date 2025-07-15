using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Notification
{
    public int NotiId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public int? UserId { get; set; }

    public string? RoleTarget { get; set; }

    public DateTime? CreatedAt { get; set; }
    public int? UserConferenceRoleId { get; set; }
    public virtual UserConferenceRole? UserConferenceRole { get; set; }

    public virtual ICollection<NotificationStatus> NotificationStatuses { get; set; } = new List<NotificationStatus>();

    public virtual User? User { get; set; }
}
