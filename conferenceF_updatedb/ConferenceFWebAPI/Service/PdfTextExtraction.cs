using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using System.Text.RegularExpressions;

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
            var glyphInfos = renderInfo.GetCharacterRenderInfos();

            List<TextRenderInfo> currentWordGlyphs = new List<TextRenderInfo>();

            foreach (var glyph in glyphInfos)
            {
                string charText = glyph.GetText();

                // Nếu gặp khoảng trắng thì kết thúc từ hiện tại
                if (string.IsNullOrWhiteSpace(charText))
                {
                    AddWordFromGlyphs(currentWordGlyphs);
                    currentWordGlyphs.Clear();
                }
                else
                {
                    currentWordGlyphs.Add(glyph);
                }
            }

            // Thêm từ cuối cùng (nếu có)
            AddWordFromGlyphs(currentWordGlyphs);
        }

        private void AddWordFromGlyphs(List<TextRenderInfo> glyphs)
        {
            if (glyphs.Count == 0) return;

            string word = string.Concat(glyphs.Select(g => g.GetText()))
                .Trim('.', ',', ';', ':', '!', '?', '"', '\'', '(', ')');

            if (string.IsNullOrEmpty(word)) return;

            var first = glyphs.First().GetBaseline().GetStartPoint();
            var last = glyphs.Last().GetAscentLine().GetEndPoint();

            float x = first.Get(0);
            float y = first.Get(1);
            float width = last.Get(0) - x;
            float height = glyphs.First().GetAscentLine().GetEndPoint().Get(1) - y;

            WordsPositions.Add(new WordPosition
            {
                Word = word,
                BoundingBox = new Rectangle(x, y, width, height)
            });
        }

        public ICollection<EventType> GetSupportedEvents() => null;

        public string GetResultantText()
        {
            return string.Join(" ", WordsPositions.Select(wp => wp.Word));
        }
    }
}
