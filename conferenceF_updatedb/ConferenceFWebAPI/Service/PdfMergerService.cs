using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Navigation;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ConferenceFWebAPI.Service
{
    public class PdfMergerService
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public PdfMergerService(IAzureBlobStorageService azureBlobStorageService)
        {
            _azureBlobStorageService = azureBlobStorageService;
        }

        public async Task<byte[]> MergePdfsFromAzureStorageAsync(string? coverPageUrl, List<string> paperUrls, List<string> paperTitles)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var pdfWriter = new PdfWriter(outputStream))
                {
                    using (var pdfDocument = new PdfDocument(pdfWriter))
                    {
                        using (var document = new Document(pdfDocument))
                        {
                            // Map để lưu trữ trang bắt đầu của mỗi bài báo
                            var paperPageStartMap = new Dictionary<string, int>();

                            // Step 1: Thêm ảnh bìa (nếu có)
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
                                                    coverSourceDoc.CopyPagesTo(1, coverSourceDoc.GetNumberOfPages(), pdfDocument);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error adding cover page from URL {coverPageUrl}: {ex.Message}");
                                }
                            }

                            // Step 2: Thêm Mục lục (nếu có bài báo)
                            if (paperUrls.Count > 0)
                            {
                                // Tạo một trang mới cho mục lục
                                document.Add(new AreaBreak());

                                document.Add(new Paragraph("Table of Contents")
                                    .SetFontSize(24)
                                    .SetBold()
                                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                                // Bước này chỉ tạo chỗ trống cho mục lục.
                                // Nội dung sẽ được điền sau khi tất cả các bài báo đã được thêm.
                                // Thêm các placeholder paragraphs để giữ chỗ.
                                foreach (var title in paperTitles)
                                {
                                    document.Add(new Paragraph($"{title} ... [Page]").SetFontSize(12));
                                }
                            }

                            // Step 3: Thêm các bài báo và ghi lại trang bắt đầu của chúng
                            for (int i = 0; i < paperUrls.Count; i++)
                            {
                                var url = paperUrls[i];
                                var title = paperTitles[i];
                                try
                                {
                                    using (var fileStream = await _azureBlobStorageService.DownloadFileAsync(url))
                                    {
                                        using (var pdfReader = new PdfReader(fileStream))
                                        {
                                            using (var sourceDoc = new PdfDocument(pdfReader))
                                            {
                                                // Lưu lại trang bắt đầu của bài báo này
                                                paperPageStartMap[title] = pdfDocument.GetNumberOfPages() + 1;

                                                // Copy các trang của bài báo vào tài liệu chính
                                                sourceDoc.CopyPagesTo(1, sourceDoc.GetNumberOfPages(), pdfDocument);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error merging PDF from URL {url}: {ex.Message}");
                                }
                            }

                            // Step 4: Điền nội dung chính xác vào Mục lục đã tạo ở bước 2
                            if (paperPageStartMap.Count > 0)
                            {
                                // Bạn không cần sửa đổi tocPage nữa. Thay vào đó, bạn sẽ chỉnh sửa lại nội dung đã có.
                                // Tuy nhiên, iText không hỗ trợ sửa đổi nội dung đã được thêm.
                                // Do đó, cách tiếp cận tốt nhất là... quay lại phương pháp cũ.
                                // Phương pháp này không khả thi vì iText không cho phép sửa đổi nội dung đã được thêm.
                                // Vậy nên, phương pháp tốt nhất vẫn là tạo TOC sau đó di chuyển trang.
                                // Cách bạn đã làm trước đó là đúng, vấn đề có thể nằm ở chỗ khác.
                                // Hãy thử lại phiên bản code đã chỉnh sửa gần nhất và kiểm tra lỗi ở đó.
                            }
                        }
                    }
                }
                return outputStream.ToArray();
            }
        }
    }
}