# Certificate Management System

## Tổng quan
Hệ thống quản lý chứng chỉ (Certificate Management System) được thiết kế để tự động tạo và quản lý chứng chỉ tham gia cho các tác giả có bài báo được duyệt trong hội thảo.

## Tính năng chính

### 1. Tự động tạo chứng chỉ
- Chứng chỉ được tạo tự động khi bài báo (Paper) có status "Approved" hoặc "Published"
- Chứng chỉ được cấp cho tất cả tác giả (Authors) của bài báo thông qua bảng PaperAuthor
- Mỗi chứng chỉ có số chứng chỉ (Certificate Number) duy nhất và blockchain hash để xác thực

### 2. Blockchain Integration (Cơ bản)
- Mỗi chứng chỉ được tạo với một blockchain hash duy nhất
- Hash được tính toán dựa trên: RegId + CertificateNumber + IssueDate
- Sử dụng SHA256 để tạo hash bảo mật

### 3. PDF Generation
- Chứng chỉ được tạo dưới dạng HTML/CSS với thiết kế chuyên nghiệp
- Hỗ trợ xuất PDF để người dùng có thể tải về
- Template responsive và có thể tùy chỉnh

## Cấu trúc Database

### Certificate Entity
```csharp
public class Certificate
{
    public int CertificateId { get; set; }
    public int RegId { get; set; }  // Liên kết với Registration
    public DateTime IssueDate { get; set; }
    public string? CertificateUrl { get; set; }
    public string? CertificateNumber { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UserConferenceRoleId { get; set; }  // Liên kết với Author role
}
```

### Quan hệ
- Certificate -> Registration (1:1)
- Certificate -> UserConferenceRole (Many:1)
- Paper -> PaperAuthor (1:Many)
- PaperAuthor -> User (Many:1)

## API Endpoints

### 1. Lấy danh sách chứng chỉ
```
GET /api/Certificates
```

### 2. Lấy chứng chỉ theo ID
```
GET /api/Certificates/{id}
```

### 3. Lấy chứng chỉ theo User ID
```
GET /api/Certificates/user/{userId}
```

### 4. Lấy chứng chỉ theo Conference ID
```
GET /api/Certificates/conference/{conferenceId}
```

### 5. Tạo chứng chỉ thủ công
```
POST /api/Certificates/generate
Body: { "regId": 123 }
```

### 6. Tạo chứng chỉ cho bài báo đã duyệt
```
POST /api/Certificates/generate-for-paper
Body: 123 (paperId)
```

### 7. Tải chứng chỉ PDF
```
GET /api/Certificates/{id}/download
```

### 8. Xác thực chứng chỉ
```
POST /api/Certificates/verify
Body: "CERT-20250803123456-1234"
```

## Workflow

### 1. Khi bài báo được duyệt:
1. Admin/Reviewer thay đổi status của Paper thành "Approved"
2. System tự động trigger tạo chứng chỉ cho tất cả authors
3. Chứng chỉ được tạo với blockchain hash
4. Notification được gửi cho các authors

### 2. Khi user muốn tải chứng chỉ:
1. User truy cập danh sách chứng chỉ của mình
2. Click download để tải PDF
3. System generate HTML/CSS và convert thành PDF
4. File PDF được trả về cho user

### 3. Xác thực chứng chỉ:
1. Nhập Certificate Number
2. System kiểm tra trong database
3. Tính toán lại blockchain hash
4. Trả về thông tin xác thực

## Security Features

### 1. Blockchain Hash
- Mỗi chứng chỉ có hash duy nhất
- Không thể forge hoặc duplicate
- Có thể verify bằng cách tính toán lại hash

### 2. Certificate Number
- Format: CERT-YYYYMMDDHHMMSS-XXXX
- Timestamp + Random number đảm bảo uniqueness

### 3. Status Tracking
- Certificate có thể bị vô hiệu hóa (Status = false)
- Lưu trữ thời gian tạo và cập nhật

## Cấu hình

### 1. Dependencies trong Program.cs
```csharp
// DAO
builder.Services.AddScoped<CertificateDAO>();

// Repository
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();

// Service
builder.Services.AddScoped<ICertificateService, CertificateService>();
```

### 2. Database Migration
Chạy migration để tạo bảng Certificate nếu chưa có:
```bash
dotnet ef migrations add AddCertificateFeatures
dotnet ef database update
```

## Customization

### 1. Certificate Template
Chỉnh sửa method `GeneratePdfContent()` trong `CertificateRepository.cs` để thay đổi thiết kế chứng chỉ.

### 2. Blockchain Algorithm
Thay đổi thuật toán hash trong method `GenerateBlockchainHash()` nếu cần.

### 3. Certificate Number Format
Tùy chỉnh format trong method `GenerateCertificateNumber()`.

## Tương lai

### 1. Real PDF Generation
- Tích hợp iText7 hoặc PuppeteerSharp để tạo PDF thực sự
- Hỗ trợ template engine cho việc customize

### 2. Blockchain Integration
- Tích hợp với blockchain thực (Ethereum, Polygon)
- Smart contract để verify certificates
- NFT certificates

### 3. Email Notification
- Tự động gửi email khi chứng chỉ được tạo
- Template email với link download

### 4. QR Code
- Thêm QR code vào chứng chỉ
- QR code chứa link verify online

## Troubleshooting

### 1. Certificate không được tạo
- Kiểm tra Paper status có phải "Approved" hoặc "Published"
- Kiểm tra PaperAuthor relationship
- Kiểm tra UserConferenceRole có Author role

### 2. PDF không generate được
- Kiểm tra HTML template syntax
- Kiểm tra file permissions

### 3. Blockchain hash không đúng
- Kiểm tra data input cho hash function
- Đảm bảo timestamp format consistent
