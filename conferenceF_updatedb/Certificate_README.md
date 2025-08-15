# Certificate Management System

## Tổng Quan

Hệ thống Certificate Management được tích hợp vào Unified Conference Management System để tự động tạo và quản lý chứng chỉ tham gia hội thảo cho người dùng sau khi họ hoàn thành thanh toán.

## Tính Năng Chính

### 1. Tự Động Tạo Certificate
- **Trigger**: Certificate được tự động tạo sau khi payment có trạng thái "PAID"
- **Điều kiện**: User phải có registration hợp lệ cho conference
- **Blockchain**: Mỗi certificate có unique hash được tạo từ SHA256

### 2. CRUD Operations
- **Create**: Tạo certificate mới (tự động hoặc thủ công)
- **Read**: Xem danh sách certificate, theo user, theo conference
- **Update**: Cập nhật thông tin certificate
- **Delete**: Xóa certificate

### 3. PDF Generation
- Certificate được xuất ra file PDF với layout chuyên nghiệp
- Bao gồm thông tin blockchain hash để verification
- Template có thể tùy chỉnh

### 4. Blockchain Verification
- Mỗi certificate có unique blockchain hash
- Hash được tạo từ: RegId + CertificateNumber + IssueDate
- Có thể verify tính hợp lệ của certificate thông qua hash

## API Endpoints

### Certificate Controller

#### GET /api/Certificates
Lấy tất cả certificates
```json
Response: [
  {
    "certificateId": 1,
    "regId": 123,
    "issueDate": "2025-08-03T10:00:00Z",
    "certificateUrl": "/certificates/cert_123.pdf",
    "certificateNumber": "CERT-20250803100000-1234",
    "status": true,
    "createdAt": "2025-08-03T10:00:00Z",
    "userName": "John Doe",
    "userEmail": "john@example.com",
    "conferenceTitle": "Tech Conference 2025"
  }
]
```

#### GET /api/Certificates/{id}
Lấy certificate theo ID

#### GET /api/Certificates/user/{userId}
Lấy tất cả certificates của user

#### GET /api/Certificates/conference/{conferenceId}
Lấy tất cả certificates của conference

#### POST /api/Certificates/generate
Tạo certificate cho registration
```json
Request: {
  "regId": 123
}
```

#### GET /api/Certificates/{id}/download
Download PDF certificate

#### POST /api/Certificates/verify
Verify certificate bằng certificate number
```json
Request: "CERT-20250803100000-1234"
Response: {
  "certificateNumber": "CERT-20250803100000-1234",
  "blockchainHash": "a1b2c3d4e5f6...",
  "isValid": true,
  "verificationMessage": "Certificate is valid. Issued to John Doe for Tech Conference 2025"
}
```

### Payment Controller

#### POST /api/Payments/{id}/complete
Hoàn thành payment và tự động tạo certificate
```json
Response: {
  "message": "Payment completed successfully. Certificate generated.",
  "paymentId": 456,
  "certificateId": 123,
  "certificateNumber": "CERT-20250803100000-1234"
}
```

## Cách Sử Dụng

### 1. Workflow Cơ Bản

1. **User đăng ký conference** → Tạo Registration
2. **User thanh toán** → Tạo Payment
3. **Payment hoàn thành** → Tự động tạo Certificate
4. **User download certificate** → Lấy file PDF

### 2. Tạo Certificate Thủ Công

```csharp
// Trong controller hoặc service
var certificate = await _certificateRepository.GenerateCertificate(registrationId);
```

### 3. Verify Certificate

```csharp
// Verify certificate bằng number
var isValid = await _certificateService.ValidateCertificate("CERT-20250803100000-1234");
```

### 4. Background Processing

Hệ thống sử dụng Hangfire để xử lý tạo certificate trong background:

```csharp
// Schedule certificate generation
_backgroundCertificateService.EnqueueCertificateGeneration(paymentId);
```

## Database Schema

### Certificate Table
```sql
CREATE TABLE Certificate (
    CertificateId INT IDENTITY(1,1) PRIMARY KEY,
    RegId INT NOT NULL,
    IssueDate DATETIME NOT NULL,
    CertificateUrl NVARCHAR(500),
    CertificateNumber NVARCHAR(100),
    Status BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UserConferenceRoleId INT,
    FOREIGN KEY (RegId) REFERENCES Registration(RegId),
    FOREIGN KEY (UserConferenceRoleId) REFERENCES UserConferenceRole(Id)
);
```

## Blockchain Integration

### Basic Implementation
- **Hash Algorithm**: SHA256
- **Data Input**: `RegId_CertificateNumber_IssueDate`
- **Output**: Hex string (lowercase)

### Future Enhancements
- Integration với public blockchain (Ethereum, Polygon)
- Smart contracts cho certificate management
- NFT certificates
- Decentralized verification

## Security Features

1. **Unique Certificate Numbers**: Timestamp + Random number
2. **Blockchain Hash**: Tamper-proof verification
3. **Status Management**: Enable/disable certificates
4. **Access Control**: API endpoints có thể thêm authorization

## Installation & Setup

1. **Database Migration**: Certificate table đã có trong migration
2. **Dependencies**: Đã đăng ký trong Program.cs
3. **Configuration**: Không cần config đặc biệt

## Testing

### Unit Tests
```csharp
[Test]
public async Task GenerateCertificate_WithValidRegistration_ReturnsValid()
{
    // Arrange
    var regId = 123;
    
    // Act
    var certificate = await _certificateRepository.GenerateCertificate(regId);
    
    // Assert
    Assert.IsNotNull(certificate);
    Assert.AreEqual(regId, certificate.RegId);
}
```

### Integration Tests
- Test payment completion flow
- Test certificate generation
- Test PDF download
- Test verification

## Troubleshooting

### Common Issues

1. **Certificate không tự động tạo**
   - Kiểm tra payment status = "PAID"
   - Kiểm tra registration tồn tại
   - Kiểm tra Hangfire running

2. **PDF generation lỗi**
   - Kiểm tra thư viện PDF đã cài đặt
   - Kiểm tra permissions để tạo file

3. **Blockchain hash không match**
   - Kiểm tra data format cho hash
   - Kiểm tra thứ tự các trường

## Future Roadmap

1. **PDF Templates**: Customizable certificate templates
2. **Email Integration**: Tự động gửi certificate qua email
3. **Bulk Generation**: Tạo certificates hàng loạt
4. **Analytics**: Thống kê certificates issued
5. **Public Blockchain**: Integration với Ethereum/Polygon
6. **NFT Support**: Convert certificates to NFTs
