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

                        // Bước 2: Thêm một trang trống để làm mục lục sau này
                        int tocPageStart = pdfDocument.GetNumberOfPages() + 1;
                        pdfDocument.AddNewPage();

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
                                                // Lưu lại trang bắt đầu của bài báo này
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

                        // Bước 4: Tạo và điền nội dung Mục lục
                        using (var document = new Document(pdfDocument))
                        {
                            document.SetBottomMargin(0);
                            document.SetTopMargin(0);

                            // Di chuyển con trỏ đến trang mục lục đã tạo
                            pdfDocument.RemovePage(tocPageStart);
                            PdfPage tocPage = pdfDocument.AddNewPage(tocPageStart);

                            Document tocDocument = new Document(pdfDocument, new iText.Kernel.Geom.PageSize(tocPage.GetPageSize()));

                            tocDocument.Add(new Paragraph("Table of Contents")
                                .SetFontSize(24)
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER));

                            foreach (var entry in paperPageStartMap)
                            {
                                var title = entry.Key;
                                var pageNum = entry.Value;

                                // Tạo một Paragraph mới cho mỗi mục lục
                                Paragraph tocEntry = new Paragraph()
                                    // Thêm TabStop cho Paragraph này
                                    .AddTabStops(new TabStop(1000, TabAlignment.RIGHT))
                                    // Thêm Link và Text vào Paragraph
                                    .Add(new Link(title, PdfAction.CreateGoTo(PdfExplicitDestination.CreateFit(pdfDocument.GetPage(pageNum)))))
                                    .Add(new Tab())
                                    .Add(new Text(pageNum.ToString()));

                                // Chỉ thêm Paragraph vào tài liệu một lần
                                tocDocument.Add(tocEntry);
                            }

                            tocDocument.Close();
                        }
                    }
                }
                return outputStream.ToArray();
            }
        }
    }
}