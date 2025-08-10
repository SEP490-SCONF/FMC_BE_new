using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;

namespace ConferenceFWebAPI.Service
{
    public class PdfService
    {
        private readonly HttpClient _httpClient;

        public PdfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ExtractTextFromPdfAsync(string pdfUrl)
        {
            if (string.IsNullOrEmpty(pdfUrl))
            {
                throw new ArgumentException("PDF URL cannot be null or empty.");
            }

            try
            {
                // Tải tệp PDF từ URL vào một Stream
                var response = await _httpClient.GetAsync(pdfUrl);
                response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu phản hồi không thành công

                using (var pdfStream = await response.Content.ReadAsStreamAsync())
                {
                    var text = new StringBuilder();

                    using (var reader = new PdfReader(pdfStream))
                    using (var pdfDocument = new PdfDocument(reader))
                    {
                        for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                        {
                            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                            string currentPageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum), strategy);
                            text.Append(currentPageText);
                        }
                    }
                    return text.ToString();
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to download PDF from {pdfUrl}. Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while processing the PDF file. Error: {ex.Message}", ex);
            }
        }
    }
}
