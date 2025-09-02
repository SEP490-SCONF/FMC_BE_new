using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Navigation;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using iText.Kernel.Utils;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;

namespace ConferenceFWebAPI.Service
{
    public class PdfMergerService
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public PdfMergerService(IAzureBlobStorageService azureBlobStorageService)
        {
            _azureBlobStorageService = azureBlobStorageService;
        }

        public async Task<byte[]> MergePdfsFromAzureStorageAsync(string coverPageUrl, List<string> paperUrls, List<string> paperTitles)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var pdfWriter = new PdfWriter(outputStream))
                {
                    using (var pdfDocument = new PdfDocument(pdfWriter))
                    {
                        var paperPageStartMap = new Dictionary<string, int>();
                        PdfMerger merger = new PdfMerger(pdfDocument);

                        // Bước 1: Thêm ảnh bìa (nếu có)
                        if (!string.IsNullOrEmpty(coverPageUrl))
                        {
                            try
                            {
                                using (var coverStream = await _azureBlobStorageService.DownloadFileAsync(coverPageUrl))
                                {
                                    if (coverStream != null && coverStream.Length > 0)
                                    {
                                        using (var coverPdfReader = new PdfReader(coverStream))
                                        {
                                            using (var coverSourceDoc = new PdfDocument(coverPdfReader))
                                            {
                                                merger.Merge(coverSourceDoc, 1, coverSourceDoc.GetNumberOfPages());
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error adding cover page: {ex.Message}");
                            }
                        }

                        // Bước 2: Thêm một trang trống cho mục lục ngay sau trang bìa
                        pdfDocument.AddNewPage();
                        int tocPageNumber = pdfDocument.GetNumberOfPages();

                        // Bước 3: Hợp nhất các bài báo và ghi lại trang bắt đầu của chúng
                        for (int i = 0; i < paperUrls.Count; i++)
                        {
                            var url = paperUrls[i];
                            var title = paperTitles[i];
                            try
                            {
                                using (var fileStream = await _azureBlobStorageService.DownloadFileAsync(url))
                                {
                                    if (fileStream != null && fileStream.Length > 0)
                                    {
                                        using (var pdfReader = new PdfReader(fileStream))
                                        {
                                            using (var sourceDoc = new PdfDocument(pdfReader))
                                            {
                                                paperPageStartMap[title] = pdfDocument.GetNumberOfPages() + 1;
                                                merger.Merge(sourceDoc, 1, sourceDoc.GetNumberOfPages());
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error merging PDF from URL {url}: {ex.Message}");
                            }
                        }

                        // Bước 4: Quay lại trang mục lục và điền nội dung
                        PdfPage tocPage = pdfDocument.GetPage(tocPageNumber);
                        using (Document tocDocument = new Document(pdfDocument))
                        {
                            // Thiết lập con trỏ tài liệu để bắt đầu từ trang mục lục.
                            // iText sẽ tự động ngắt trang nếu nội dung vượt quá một trang.
                            tocDocument.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                            tocDocument.Add(new Paragraph("Table of Contents")
                                .SetFontSize(24)
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER));

                            foreach (var entry in paperPageStartMap)
                            {
                                var title = entry.Key;
                                var pageNum = entry.Value;

                                Paragraph tocEntry = new Paragraph()
                                    .AddTabStops(new TabStop(500, TabAlignment.RIGHT)) // Giá trị 1000 có thể quá lớn
                                    .Add(new Link(title, PdfAction.CreateGoTo(PdfExplicitDestination.CreateFit(pdfDocument.GetPage(pageNum)))))
                                    .Add(new Tab())
                                    .Add(new Text(pageNum.ToString()));

                                tocDocument.Add(tocEntry);
                            }
                        }
                    }
                }
                return outputStream.ToArray();
            }
        }
    }
}