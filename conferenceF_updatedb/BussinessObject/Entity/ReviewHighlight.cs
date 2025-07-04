using System;
using System.Collections.Generic;

namespace BussinessObject.Entity;

public partial class ReviewHighlight
{
    public int HighlightId { get; set; }

    public int ReviewId { get; set; }

    public int? PageIndex { get; set; }   
    public double ?Left { get; set; }       
    public double ?Top { get; set; }         
    public double ?Width { get; set; }      
    public double ?Height { get; set; }    



    public string? TextHighlighted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Review Review { get; set; } = null!;

    public virtual ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}
