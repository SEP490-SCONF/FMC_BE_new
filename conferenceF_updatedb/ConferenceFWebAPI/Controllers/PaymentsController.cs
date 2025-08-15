using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICertificateService _certificateService;
        private readonly IMapper _mapper;

        public PaymentsController(
            IPaymentRepository paymentRepository,
            ICertificateService certificateService,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _certificateService = certificateService;
            _mapper = mapper;
        }

        // GET: api/Payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            try
            {
                var payments = await _paymentRepository.GetAll();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetById(id);
                if (payment == null)
                {
                    return NotFound($"Payment with ID {id} not found.");
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Payments/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByUserId(int userId)
        {
            try
            {
                var payments = await _paymentRepository.GetByUserId(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] Payment payment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                payment.CreatedAt = DateTime.UtcNow;
                await _paymentRepository.Add(payment);

                return CreatedAtAction(nameof(GetPayment), new { id = payment.PayId }, payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Payments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
        {
            try
            {
                if (id != payment.PayId)
                {
                    return BadRequest("Payment ID mismatch.");
                }

                var existingPayment = await _paymentRepository.GetById(id);
                if (existingPayment == null)
                {
                    return NotFound("Payment not found.");
                }

                await _paymentRepository.Update(payment);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Payments/5/complete
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompletePayment(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetById(id);
                if (payment == null)
                {
                    return NotFound("Payment not found.");
                }

                // Cập nhật trạng thái payment
                payment.PayStatus = "PAID";
                payment.PaidAt = DateTime.UtcNow;
                await _paymentRepository.Update(payment);

                // Tự động tạo certificate nếu có registration
                if (payment.RegId.HasValue)
                {
                    try
                    {
                        var certificate = await _certificateService.GenerateCertificateOnPaymentComplete(id);
                        if (certificate != null)
                        {
                            return Ok(new
                            {
                                Message = "Payment completed successfully. Certificate generated.",
                                PaymentId = payment.PayId,
                                CertificateId = certificate.CertificateId,
                                CertificateNumber = certificate.CertificateNumber
                            });
                        }
                    }
                    catch (Exception certEx)
                    {
                        // Log certificate generation error but don't fail the payment completion
                        Console.WriteLine($"Certificate generation failed: {certEx.Message}");
                    }
                }

                return Ok(new
                {
                    Message = "Payment completed successfully.",
                    PaymentId = payment.PayId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetById(id);
                if (payment == null)
                {
                    return NotFound("Payment not found.");
                }

                await _paymentRepository.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
