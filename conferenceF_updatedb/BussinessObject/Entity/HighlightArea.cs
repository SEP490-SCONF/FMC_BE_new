using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObject.Entity
{
    public class HighlightArea
    {
        public int HighlightAreaId { get; set; }  // Khóa chính

        public int HighlightId { get; set; }      // Liên kết với ReviewHighlight

        public int? PageIndex { get; set; }      // Chỉ số trang

        public double? Left { get; set; }        // Vị trí bên trái

        public double? Top { get; set; }         // Vị trí phía trên

        public double? Width { get; set; }       // Chiều rộng

        public double? Height { get; set; }      // Chiều cao

        public string? TextHighlighted { get; set; }  // Văn bản được highlight

        // Liên kết với ReviewHighlight
        public virtual ReviewHighlight ReviewHighlight { get; set; } = null!;
    }

}
