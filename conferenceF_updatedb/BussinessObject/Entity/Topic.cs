using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class Topic
{
    public int TopicId { get; set; }

    public bool? Status { get; set; }

    public string TopicName { get; set; } = null!;

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();

    public virtual ICollection<Conference> Conferences { get; set; } = new List<Conference>();
}
