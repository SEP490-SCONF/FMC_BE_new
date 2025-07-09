using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int RegId { get; set; }

    public DateTime IssueDate { get; set; }

    public string? CertificateUrl { get; set; }

    public string? CertificateNumber { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? UserConferenceRoleId { get; set; }
    public virtual UserConferenceRole? UserConferenceRole { get; set; }

    public virtual Registration Reg { get; set; } = null!;
}
