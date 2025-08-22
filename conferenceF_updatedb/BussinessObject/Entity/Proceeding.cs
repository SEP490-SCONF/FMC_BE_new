using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Proceeding
{
    public int ProceedingId { get; set; }

    public int ConferenceId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? FilePath { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? PublishedBy { get; set; }

    // Các thuộc tính mới được thêm vào
    public string? Status { get; set; }

    public string? Version { get; set; }

    public string? Doi { get; set; }
    public string? CoverPageUrl { get; set; } // Thêm thuộc tính này để lưu URL của ảnh bìa

    public virtual Conference Conference { get; set; } = null!;

    public virtual User? PublishedByNavigation { get; set; }

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}