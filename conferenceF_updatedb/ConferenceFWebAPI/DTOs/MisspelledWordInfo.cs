using System.Drawing;

namespace ConferenceFWebAPI.DTOs
{
    public class MisspelledWordInfo
    {
        public string Word { get; set; }
        public int PageNumber { get; set; }
        public Rectangle BoundingBox { get; set; }
    }
}
