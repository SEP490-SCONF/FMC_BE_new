using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Payment
{
    public int PayId { get; set; }

    public int UserId { get; set; }

    public int ConferenceId { get; set; }

    public int? RegId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? PayStatus { get; set; }

    public string? PayOsOrderCode { get; set; }

    public string? PayOsCheckoutUrl { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? PaperId { get; set; }

    public string? Purpose { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual Paper? Paper { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual Registration? Reg { get; set; }

    public virtual User User { get; set; } = null!;
}
