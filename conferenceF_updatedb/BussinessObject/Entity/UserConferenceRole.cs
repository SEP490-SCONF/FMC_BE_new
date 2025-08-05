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
    // Thông tin committee
    public string? GroupName { get; set; }
    public string? SpecificTitle { get; set; }
    public string? Affiliation { get; set; }

    // Quản lý hiển thị & xác nhận
    public bool IsPublic { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? ConfirmationToken { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Thông tin bổ sung
    public string? Expertise { get; set; }
    public string? DisplayNameOverride { get; set; }
    public int? SortOrder { get; set; }

    // Thời gian tạo & cập nhật
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Conference Conference { get; set; } = null!;

    public virtual ConferenceRole ConferenceRole { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
