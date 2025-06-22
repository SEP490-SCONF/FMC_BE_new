using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Registration
{
    public int RegId { get; set; }

    public int UserId { get; set; }

    public int ConferenceId { get; set; }

    public DateTime? RegisteredAt { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;
}
