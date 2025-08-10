using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace ConferenceFWebAPI.Services.PdfTextExtraction
{
    public class WordPosition
    {
        public string Word { get; set; }
        public int PageNumber { get; set; }
        public Rectangle BoundingBox { get; set; }
    }

    public class LocationTextExtractionStrategyWithPosition : ITextExtractionStrategy
    {
        public List<WordPosition> WordsPositions { get; } = new List<WordPosition>();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT) return;

            var renderInfo = (TextRenderInfo)data;
            string text = renderInfo.GetText();
            if (string.IsNullOrWhiteSpace(text)) return;

            var baseline = renderInfo.GetBaseline();
            var ascentLine = renderInfo.GetAscentLine();
            var rect = new Rectangle(
                baseline.GetStartPoint().Get(0),
                baseline.GetStartPoint().Get(1),
                ascentLine.GetEndPoint().Get(0) - baseline.GetStartPoint().Get(0),
                ascentLine.GetEndPoint().Get(1) - baseline.GetStartPoint().Get(1)
            );

            WordsPositions.Add(new WordPosition
            {
                Word = text,
                BoundingBox = rect
            });
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }

        public string GetResultantText()
        {
            return string.Join(" ", WordsPositions.Select(wp => wp.Word));
        }
    }
}
