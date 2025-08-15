using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Repository.Repository
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly CertificateDAO _certificateDao;

        public CertificateRepository(CertificateDAO certificateDao)
        {
            _certificateDao = certificateDao;
        }

        public async Task<IEnumerable<Certificate>> GetAll()
        {
            return await _certificateDao.GetAll();
        }

        public async Task<Certificate> GetById(int id)
        {
            return await _certificateDao.GetById(id);
        }

        public async Task Add(Certificate entity)
        {
            await _certificateDao.Add(entity);
        }

        public async Task Update(Certificate entity)
        {
            await _certificateDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _certificateDao.Delete(id);
        }

        public async Task<Certificate> GetByRegistrationId(int regId)
        {
            return await _certificateDao.GetByRegistrationId(regId);
        }

        public async Task<IEnumerable<Certificate>> GetByUserId(int userId)
        {
            return await _certificateDao.GetByUserId(userId);
        }

        public async Task<IEnumerable<Certificate>> GetByConferenceId(int conferenceId)
        {
            return await _certificateDao.GetByConferenceId(conferenceId);
        }

        public async Task<bool> ExistsByRegistrationId(int regId)
        {
            return await _certificateDao.ExistsByRegistrationId(regId);
        }

        public async Task<bool> IsPaymentCompleted(int regId)
        {
            return await _certificateDao.IsPaymentCompleted(regId);
        }

        public async Task<Certificate> GenerateCertificate(int regId)
        {
            // Bỏ kiểm tra payment, chỉ kiểm tra registration
            // Kiểm tra xem certificate đã tồn tại chưa
            var existingCertificate = await GetByRegistrationId(regId);
            if (existingCertificate != null)
            {
                return existingCertificate;
            }

            // Tạo certificate mới
            var certificate = new Certificate
            {
                RegId = regId,
                IssueDate = DateTime.UtcNow,
                CertificateNumber = GenerateCertificateNumber(),
                Status = true,
                CreatedAt = DateTime.UtcNow
            };

            await Add(certificate);

            // Tạo PDF URL placeholder
            certificate.CertificateUrl = $"/certificates/certificate_{certificate.CertificateId}_{certificate.CertificateNumber}.pdf";
            await Update(certificate);

            return certificate;
        }

        // Thêm method mới để generate certificate cho author của paper đã approved
        public async Task<Certificate> GenerateCertificateForApprovedPaper(int paperId, int authorId)
        {
            // Kiểm tra xem đã có certificate cho paper này và author này chưa
            var certificates = await GetAll();
            var existingCertificate = certificates.FirstOrDefault(c => 
                c.Reg != null && 
                c.Reg.UserId == authorId && 
                c.UserConferenceRoleId != null);

            if (existingCertificate != null)
            {
                return existingCertificate;
            }

            // Tạo certificate mới cho author
            var certificate = new Certificate
            {
                RegId = 0, // Temporary, sẽ cần tạo registration tạm hoặc sử dụng cách khác
                IssueDate = DateTime.UtcNow,
                CertificateNumber = GenerateCertificateNumber(),
                Status = true,
                CreatedAt = DateTime.UtcNow,
                UserConferenceRoleId = null // Sẽ set sau khi tìm được Author role
            };

            await Add(certificate);

            certificate.CertificateUrl = $"/certificates/certificate_{certificate.CertificateId}_{certificate.CertificateNumber}.pdf";
            await Update(certificate);

            return certificate;
        }

        public async Task<byte[]> GenerateCertificatePdf(int certificateId)
        {
            var certificate = await GetById(certificateId);
            if (certificate == null)
            {
                throw new ArgumentException("Certificate not found.");
            }

            // Placeholder PDF generation - sẽ implement với iText7 sau
            var pdfContent = GeneratePdfContent(certificate);
            return Encoding.UTF8.GetBytes(pdfContent);
        }

        private string GeneratePdfContent(Certificate certificate)
        {
            // Tạo HTML/CSS content cho certificate
            var blockchainHash = GenerateBlockchainHash(certificate);
            
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Certificate of Participation</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;700&family=Open+Sans:wght@300;400;600&display=swap');
        
        body {{
            margin: 0;
            padding: 40px;
            font-family: 'Open Sans', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
        }}
        
        .certificate {{
            width: 800px;
            height: 600px;
            background: linear-gradient(45deg, #f8f9fa, #e9ecef);
            border: 15px solid #2c3e50;
            border-radius: 20px;
            padding: 40px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.3);
            position: relative;
            overflow: hidden;
        }}
        
        .certificate::before {{
            content: '';
            position: absolute;
            top: 20px;
            left: 20px;
            right: 20px;
            bottom: 20px;
            border: 3px solid #34495e;
            border-radius: 10px;
            pointer-events: none;
        }}
        
        .header {{
            text-align: center;
            margin-bottom: 30px;
            position: relative;
            z-index: 2;
        }}
        
        .logo {{
            width: 80px;
            height: 80px;
            background: #3498db;
            border-radius: 50%;
            margin: 0 auto 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 24px;
            font-weight: bold;
        }}
        
        .system-title {{
            font-family: 'Playfair Display', serif;
            font-size: 18px;
            color: #2c3e50;
            font-weight: 600;
            margin-bottom: 5px;
            letter-spacing: 2px;
        }}
        
        .certificate-title {{
            font-family: 'Playfair Display', serif;
            font-size: 36px;
            color: #e74c3c;
            font-weight: 700;
            margin: 20px 0;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.1);
        }}
        
        .content {{
            text-align: center;
            margin: 40px 0;
            position: relative;
            z-index: 2;
        }}
        
        .presented-to {{
            font-size: 16px;
            color: #7f8c8d;
            margin-bottom: 10px;
        }}
        
        .participant-name {{
            font-family: 'Playfair Display', serif;
            font-size: 32px;
            color: #2c3e50;
            font-weight: 700;
            margin: 15px 0;
            padding: 10px;
            border-bottom: 3px solid #3498db;
            display: inline-block;
        }}
        
        .for-text {{
            font-size: 16px;
            color: #7f8c8d;
            margin: 20px 0 10px;
        }}
        
        .conference-title {{
            font-family: 'Playfair Display', serif;
            font-size: 24px;
            color: #8e44ad;
            font-weight: 600;
            margin: 10px 0;
            font-style: italic;
        }}
        
        .field {{
            font-size: 14px;
            color: #95a5a6;
            margin: 20px 0;
        }}
        
        .date {{
            font-size: 16px;
            color: #34495e;
            margin: 30px 0;
            font-weight: 600;
        }}
        
        .footer {{
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 40px;
            position: relative;
            z-index: 2;
        }}
        
        .certificate-info {{
            text-align: left;
            font-size: 11px;
            color: #7f8c8d;
            line-height: 1.4;
        }}
        
        .signature {{
            text-align: right;
        }}
        
        .signature-line {{
            width: 200px;
            height: 2px;
            background: #34495e;
            margin-bottom: 5px;
        }}
        
        .signature-text {{
            font-size: 14px;
            color: #2c3e50;
            margin-bottom: 5px;
        }}
        
        .organization {{
            font-family: 'Playfair Display', serif;
            font-size: 18px;
            color: #e74c3c;
            font-weight: 600;
        }}
        
        .decorative-elements {{
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            opacity: 0.1;
            z-index: 1;
        }}
        
        .decorative-elements::before {{
            content: '🏆';
            position: absolute;
            top: 30px;
            left: 30px;
            font-size: 40px;
        }}
        
        .decorative-elements::after {{
            content: '🎓';
            position: absolute;
            bottom: 30px;
            right: 30px;
            font-size: 40px;
        }}
        
        .blockchain-hash {{
            font-family: 'Courier New', monospace;
            font-size: 8px;
            color: #95a5a6;
            word-break: break-all;
            margin-top: 5px;
        }}
    </style>
</head>
<body>
    <div class=""certificate"">
        <div class=""decorative-elements""></div>
        
        <div class=""header"">
            <div class=""logo"">UC</div>
            <div class=""system-title"">UNIFIED CONFERENCE MANAGEMENT SYSTEM</div>
            <div class=""certificate-title"">CERTIFICATE OF PARTICIPATION</div>
        </div>
        
        <div class=""content"">
            <div class=""presented-to"">is presented to</div>
            <div class=""participant-name"">{certificate.Reg?.User?.Name ?? "Participant Name"}</div>
            
            <div class=""for-text"">for actively participating in the</div>
            <div class=""conference-title"">{certificate.Reg?.Conference?.Title ?? "Conference Title"}</div>
            
            <div class=""field"">Information Technology</div>
            
            <div class=""date"">{certificate.IssueDate:MMMM dd, yyyy}</div>
        </div>
        
        <div class=""footer"">
            <div class=""certificate-info"">
                <div><strong>Certificate Number:</strong> {certificate.CertificateNumber}</div>
                <div><strong>Issue Date:</strong> {certificate.IssueDate:yyyy-MM-dd}</div>
                <div><strong>Status:</strong> {(certificate.Status ? "Valid" : "Invalid")}</div>
                <div class=""blockchain-hash""><strong>Blockchain Hash:</strong><br>{blockchainHash}</div>
            </div>
            
            <div class=""signature"">
                <div class=""signature-line""></div>
                <div class=""signature-text"">Signature</div>
                <div class=""organization"">UTE Conference</div>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCertificateNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"CERT-{timestamp}-{random}";
        }

        private string GenerateBlockchainHash(Certificate certificate)
        {
            // Tạo chuỗi data để hash
            var dataToHash = $"{certificate.RegId}_{certificate.CertificateNumber}_{certificate.IssueDate:yyyyMMddHHmmss}";
            
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        public async Task<IEnumerable<Certificate>> GetCertificatesByPaperId(int paperId)
        {
            return await _certificateDao.GetCertificatesByPaperId(paperId);
        }

    }
}
