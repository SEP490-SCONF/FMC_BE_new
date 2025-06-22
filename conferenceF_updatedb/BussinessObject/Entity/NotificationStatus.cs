using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class NotificationStatus
{
    public int Id { get; set; }

    public int NotiId { get; set; }

    public int UserId { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Notification Noti { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
