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

        //[HttpPost("create")]
        //public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentDTO dto)
        //{
        //    // Lấy userId từ JWT
        //    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        //        return Unauthorized("Invalid user context");

        //    // Tạo orderCode tự động từ paperId, conferenceId, amount
        //    int orderCode = GenerateOrderCode();

        //    // Dữ liệu gửi tới PayOS
        //    var paymentData = new PaymentData(
        //        orderCode,
        //        (int)(dto.Amount),
        //        dto.Purpose ?? $"Conference payment #{dto.ConferenceId}",
        //        new List<ItemData>
        //        {
        //            new ItemData("Conference Fee", 1, (int)(dto.Amount))
        //        },
        //        "https://fmc-fe.vercel.app/payment-cancel",
        //        $"https://fmc-fe.vercel.app/payment-success?orderCode={orderCode}"
        //    );

        //    var linkResponse = await _payOS.createPaymentLink(paymentData);

        //    // Lưu vào DB
        //    var payment = new Payment
        //    {
        //        UserId = userId,
        //        ConferenceId = dto.ConferenceId,
        //        PaperId = dto.PaperId,
        //        Purpose = dto.Purpose,
        //        Amount = dto.Amount,
        //        Currency = dto.Currency ?? "VND",
        //        PayStatus = "Pending",
        //        CreatedAt = DateTime.UtcNow,
        //        PayOsCheckoutUrl = linkResponse.checkoutUrl,
        //        PayOsOrderCode = orderCode.ToString(),
        //    };

        //    await _paymentRepository.Add(payment);

        //    return Ok(new { checkoutUrl = linkResponse.checkoutUrl, orderCode });
        //}

        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Invalid user context");

            if (dto.Fees == null || !dto.Fees.Any())
                return BadRequest("No fees provided");

            // Lấy tất cả FeeDetails từ DB
            var feeDetails = await _paymentRepository.GetFeeDetailsByIdsAsync(
                dto.Fees.Select(f => f.FeeDetailId).ToList()
            );
            if (feeDetails.Count != dto.Fees.Count)
                return BadRequest("Some FeeDetailIds are invalid");

            // Tính tổng tiền
            decimal totalAmount = 0;
            var items = new List<ItemData>();
            var purposes = new List<string>();

            foreach (var feeItem in dto.Fees)
            {
                var detail = feeDetails.First(d => d.FeeDetailId == feeItem.FeeDetailId);
                var amount = detail.Amount * feeItem.Quantity;
                totalAmount += amount;

                purposes.Add($"{detail.FeeType?.Name} x{feeItem.Quantity}");

                items.Add(new ItemData(
                    detail.FeeType?.Name ?? "Conference Fee",
                    feeItem.Quantity,
                    (int)detail.Amount
                ));
            }

            // lấy feeDetailId chính (mặc định là Registration nếu có)
            int? mainFeeDetailId = dto.Fees.FirstOrDefault()?.FeeDetailId;

            int orderCode = GenerateOrderCode();

            // Rút gọn description cho PayOS (giới hạn 25 ký tự)
            string description = string.Join(", ", purposes);
            if (description.Length > 25)
            {
                description = description.Substring(0, 25);
            }

            var paymentData = new PaymentData(
                orderCode,
                (int)totalAmount,
                description,   // gửi sang PayOS: max 25 ký tự
                items,
                "https://fmc-fe.vercel.app/payment-cancel",
                $"https://fmc-fe.vercel.app/payment-success?orderCode={orderCode}"
            );

            var linkResponse = await _payOS.createPaymentLink(paymentData);

            // Lưu DB với purpose đầy đủ
            var payment = new Payment
            {
                UserId = userId,
                ConferenceId = dto.ConferenceId,
                PaperId = dto.PaperId,
                FeeDetailId = mainFeeDetailId, // chỉ lưu được 1 loại
                Purpose = string.Join(", ", purposes), // giữ nguyên full text
                Amount = totalAmount,
                Currency = "VND",
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

            var paymentDTOs = payments.Select(p => new PaymentDTO
            {
                PayId = p.PayId,
                UserId = p.UserId,
                UserName = p.User?.Name,
                ConferenceId = p.ConferenceId,
                ConferenceName = p.Conference?.Title,
                RegId = p.RegId,
                Amount = p.Amount,
                Currency = p.Currency,
                PayStatus = p.PayStatus,
                PayOsOrderCode = p.PayOsOrderCode,
                PayOsCheckoutUrl = p.PayOsCheckoutUrl,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt,
                PaperId = p.PaperId,
                PaperTitle = p.Paper?.Title,
                Purpose = p.Purpose,
                FeeType = p.FeeDetail?.FeeType?.Name, // lấy tên FeeType
                Mode = p.FeeDetail?.Mode
            });

            return Ok(paymentDTOs);
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





        [HttpGet("hasUserPaid")]
        public async Task<IActionResult> HasUserPaidFee([FromQuery] int conferenceId, [FromQuery] int feeDetailId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Invalid user context");

            var hasPaid = await _paymentRepository.HasUserPaidFee(userId, conferenceId, feeDetailId);

            return Ok(new
            {
                UserId = userId,
                ConferenceId = conferenceId,
                FeeDetailId = feeDetailId,
                HasPaid = hasPaid
            });
        }


        [HttpGet("fee/{feeDetailId}")]
        public async Task<IActionResult> GetFeeDetail(int feeDetailId)
        {
            var feeDetail = await _paymentRepository.GetFeeDetailByIdAsync(feeDetailId);
            if (feeDetail == null)
                return NotFound("Fee detail not found.");

            return Ok(new
            {
                feeDetail.FeeDetailId,
                feeDetail.ConferenceId,
                feeDetail.Amount,
                feeDetail.Currency,
                feeDetail.Mode,
                feeDetail.Note,
                feeDetail.IsVisible,
                FeeType = feeDetail.FeeType?.Name
            });
        }

    }
}



