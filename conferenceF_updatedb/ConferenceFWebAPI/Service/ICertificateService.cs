using BussinessObject.Entity;

namespace ConferenceFWebAPI.Service
{
    public interface ICertificateService
    {
        Task<Certificate?> GenerateCertificateOnPaymentComplete(int paymentId);
        Task<byte[]> GeneratePdfCertificate(int certificateId);
        Task<bool> ValidateCertificate(string certificateNumber);
        Task<string> GetBlockchainHash(Certificate certificate);
    }
}
