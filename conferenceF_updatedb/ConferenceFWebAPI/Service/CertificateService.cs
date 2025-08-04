using BussinessObject.Entity;
using Repository;
using System.Security.Cryptography;
using System.Text;

namespace ConferenceFWebAPI.Service
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IPaperRepository _paperRepository;

        public CertificateService(
            ICertificateRepository certificateRepository,
            IPaymentRepository paymentRepository,
            IRegistrationRepository registrationRepository,
            IPaperRepository paperRepository)
        {
            _certificateRepository = certificateRepository;
            _paymentRepository = paymentRepository;
            _registrationRepository = registrationRepository;
            _paperRepository = paperRepository;
        }

        public async Task<Certificate?> GenerateCertificateOnPaymentComplete(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetById(paymentId);
                if (payment == null || payment.PayStatus != "PAID" || payment.RegId == null)
                {
                    return null;
                }

                // Kiểm tra xem certificate đã tồn tại chưa
                var existingCertificate = await _certificateRepository.GetByRegistrationId(payment.RegId.Value);
                if (existingCertificate != null)
                {
                    return existingCertificate;
                }

                // Tạo certificate mới
                return await _certificateRepository.GenerateCertificate(payment.RegId.Value);
            }
            catch (Exception ex)
            {
                // Log error và return null thay vì throw exception
                Console.WriteLine($"Error generating certificate for payment {paymentId}: {ex.Message}");
                return null;
            }
        }

        // Thêm method mới để tạo certificate khi paper được approve
        public async Task<List<Certificate>> GenerateCertificatesForApprovedPaper(int paperId)
        {
            try
            {
                var certificates = new List<Certificate>();
                
                // Get paper details
                var paper = await _paperRepository.GetPaperByIdAsync(paperId);
                if (paper == null || (paper.Status != "Approved" && paper.Status != "Published"))
                {
                    return certificates;
                }

                // Get all authors of this paper
                // Note: Cần implement method GetAuthorsByPaperId trong PaperRepository
                // Tạm thời return empty list
                
                return certificates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating certificates for paper {paperId}: {ex.Message}");
                return new List<Certificate>();
            }
        }

        public async Task<byte[]> GeneratePdfCertificate(int certificateId)
        {
            return await _certificateRepository.GenerateCertificatePdf(certificateId);
        }

        public async Task<bool> ValidateCertificate(string certificateNumber)
        {
            try
            {
                var certificates = await _certificateRepository.GetAll();
                var certificate = certificates.FirstOrDefault(c => c.CertificateNumber == certificateNumber);
                
                return certificate != null && certificate.Status;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetBlockchainHash(Certificate certificate)
        {
            return await Task.FromResult(GenerateBlockchainHash(certificate));
        }

        private string GenerateBlockchainHash(Certificate certificate)
        {
            var dataToHash = $"{certificate.RegId}_{certificate.CertificateNumber}_{certificate.IssueDate:yyyyMMddHHmmss}";
            
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
