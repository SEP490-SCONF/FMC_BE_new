using ConferenceFWebAPI.Service;
using Hangfire;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class BackgroundCertificateService
    {
        private readonly ICertificateService _certificateService;
        private readonly IPaymentRepository _paymentRepository;

        public BackgroundCertificateService(
            ICertificateService certificateService,
            IPaymentRepository paymentRepository)
        {
            _certificateService = certificateService;
            _paymentRepository = paymentRepository;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessCertificateGeneration(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetById(paymentId);
                if (payment?.PayStatus == "PAID" && payment.RegId.HasValue)
                {
                    await _certificateService.GenerateCertificateOnPaymentComplete(paymentId);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Background certificate generation failed for payment {paymentId}: {ex.Message}");
                throw; // Re-throw để Hangfire retry
            }
        }

        public void ScheduleCertificateGeneration(int paymentId, TimeSpan delay)
        {
            BackgroundJob.Schedule(() => ProcessCertificateGeneration(paymentId), delay);
        }

        public void EnqueueCertificateGeneration(int paymentId)
        {
            BackgroundJob.Enqueue(() => ProcessCertificateGeneration(paymentId));
        }
    }
}
