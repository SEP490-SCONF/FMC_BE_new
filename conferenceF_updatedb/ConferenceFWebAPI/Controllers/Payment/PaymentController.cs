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
            // Lấy userId từ JWT
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Invalid user context");

            // Tạo orderCode tự động từ paperId, conferenceId, amount
            int orderCode = GenerateOrderCode();

            // Dữ liệu gửi tới PayOS
            var paymentData = new PaymentData(
                orderCode,
                (int)(dto.Amount),
                dto.Purpose ?? $"Thanh toán hội thảo #{dto.ConferenceId}",
                new List<ItemData>
                {
                    new ItemData("Conference Fee", 1, (int)(dto.Amount))
                },
                "https://fmc-fe.vercel.app//payment-cancel",
                $"https://fmc-fe.vercel.app//payment-success?orderCode={orderCode}"
            );

            var linkResponse = await _payOS.createPaymentLink(paymentData);

            // Lưu vào DB
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
                PayOsOrderCode = orderCode.ToString(),
            };

            await _paymentRepository.Add(payment);

            return Ok(new { checkoutUrl = linkResponse.checkoutUrl, orderCode });
        }

        // Hàm tạo OrderCode
        private int GenerateOrderCode()
        {
            // Lấy số mili giây hiện tại trong ngày để tránh quá lớn
            int millis = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue);

            // Thêm 4 số random để tránh trùng
            int randomPart = Random.Shared.Next(1000, 9999);

            // Ghép lại và mod để đảm bảo <= int.MaxValue
            int orderCode = (millis + randomPart) % int.MaxValue;

            return orderCode;
        }


        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string orderCode)
        {
            var payment = await _paymentRepository.GetByOrderCode(orderCode);
            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            payment.PayStatus = "Completed";
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.Update(payment);

            return Ok(new
            {
                status = "success",
                orderCode,
                message = "Payment completed successfully"
            });
        }

        [HttpGet("cancel")]
        public async Task<IActionResult> PaymentCancel([FromQuery] string orderCode)
        {
            var payment = await _paymentRepository.GetByOrderCode(orderCode);
            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            payment.PayStatus = "Cancelled";
            await _paymentRepository.Update(payment);

            return Ok(new
            {
                status = "cancelled",
                orderCode,
                message = "Payment cancelled"
            });
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
