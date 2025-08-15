using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Payment;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Repository;
using System.Security.Claims;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly PayOS _payOS;

        public PaymentController(IPaymentRepository paymentRepository, IMapper mapper, PayOS payOS)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _payOS = payOS;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentRepository.GetAll();
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentDTO dto)
        {
            //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            //    return Unauthorized("Invalid user context");
            int userId = dto.UserId;

            // Tạo dữ liệu gửi tới PayOS
            var paymentData = new PaymentData(
                dto.OrderCode,
                (int)(dto.Amount * 1000),
                dto.Purpose ?? $"Thanh toán hội thảo #{dto.ConferenceId}",
                new List<ItemData>
                {
                    new ItemData("Conference Fee", 1, (int)(dto.Amount * 1000))
                },
                "http://localhost:5173/payment-cancel",
                $"http://localhost:5173/payment-success?orderCode={dto.OrderCode}"
            );

            var linkResponse = await _payOS.createPaymentLink(paymentData);

            var payment = new Payment
            {
                UserId = userId,
                ConferenceId = dto.ConferenceId,
                PaperId = dto.PaperId,
                Purpose = dto.Purpose,
                Amount = dto.Amount,
                Currency = dto.Currency ?? "VND",
                PayStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                PayOsCheckoutUrl = linkResponse.checkoutUrl,
                PayOsOrderCode =dto.OrderCode.ToString(),
            };

            await _paymentRepository.Add(payment);
            return Ok(new { checkoutUrl = linkResponse.checkoutUrl, orderCode = dto.OrderCode });
        }

        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string orderCode)
        {
            var payment = await _paymentRepository.GetByOrderCode(orderCode);
            if (payment == null) return NotFound("Payment not found");

            payment.PayStatus = "Completed";
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.Update(payment);

            return Redirect("http://localhost:5173/payment-success");
        }

        [HttpGet("cancel")]
        public async Task<IActionResult> PaymentCancel([FromQuery] string orderCode)
        {
            var payment = await _paymentRepository.GetByOrderCode(orderCode);
            if (payment == null) return NotFound("Payment not found");

            payment.PayStatus = "Cancelled";
            await _paymentRepository.Update(payment);

            return Redirect("http://localhost:5173/payment-cancel");
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetByUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Invalid user context");

            var payments = await _paymentRepository.GetByUserId(userId);
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }

        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetByConference(int conferenceId)
        {
            var payments = await _paymentRepository.GetByConferenceId(conferenceId);
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var payments = await _paymentRepository.GetByStatus(status);
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] DateTime fromDate)
        {
            var payments = await _paymentRepository.GetRecentPayments(fromDate);
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }
    }
}
