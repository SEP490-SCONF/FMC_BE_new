using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PaymentDAO
    {
        private readonly ConferenceFTestContext _context;

        public PaymentDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAll()
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Conference)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving payments.", ex);
            }
        }

        public async Task<Payment> GetById(int id)
        {
            try
            {
                return await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PayId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Payment>> GetByUserId(int userId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Conference)
                    .Include(p => p.Paper)
                    .Include(p => p.FeeDetail)
                        .ThenInclude(fd => fd.FeeType) // include FeeType
                    .Where(p => p.UserId == userId && p.PayStatus == "Completed")
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payments for user ID {userId}.", ex);
            }
        }



        public async Task Add(Payment payment)
        {
            try
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding payment.", ex);
            }
        }

        public async Task Update(Payment payment)
        {
            try
            {
                var existing = await _context.Payments.FindAsync(payment.PayId);
                if (existing == null)
                    throw new Exception($"Payment with ID {payment.PayId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(payment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating payment with ID {payment.PayId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment != null)
                {
                    _context.Payments.Remove(payment);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Payment with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting payment with ID {id}.", ex);
            }
        }
        public async Task<IEnumerable<Payment>> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Conference)
                    .Where(p => p.ConferenceId == conferenceId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payments for conference ID {conferenceId}.", ex);
            }
        }

        public async Task<IEnumerable<Payment>> GetByStatus(string status)
        {
            try
            {
                return await _context.Payments.Include(p => p.User).Include(p => p.Conference)
                    .Where(p => p.PayStatus == status)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payments with status {status}.", ex);
            }
        }

        public async Task<IEnumerable<Payment>> GetRecentPayments(DateTime fromDate)
        {
            try
            {
                return await _context.Payments.Include(p => p.User).Include(p => p.Conference)
                    .Where(p => p.CreatedAt >= fromDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving recent payments since {fromDate}.", ex);
            }
        }

        public async Task<Payment?> GetByOrderCode(String orderCode)
        {
            try
            {
                return await _context.Payments.Include(p => p.User).Include(p => p.Conference)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PayOsOrderCode == orderCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment with order code {orderCode}.", ex);
            }
        }
        public async Task<Payment?> GetLatestPendingByUserId(int userId)
        {
            try
            {
                return await _context.Payments
                    .Where(p => p.UserId == userId && p.PayStatus == "Pending")
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving latest pending payment for user ID {userId}.", ex);
            }
        }

        public async Task<bool> HasUserPaidFee(int userId, int conferenceId, int feeDetailId)
        {
            try
            {
                return await _context.Payments
                    .AnyAsync(p => p.UserId == userId
                                && p.ConferenceId == conferenceId
                                && p.FeeDetailId == feeDetailId
                                && p.PayStatus == "Completed");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking payment for user {userId} in conference {conferenceId}, feeDetailId {feeDetailId}.", ex);
            }
        }


        public async Task<FeeDetail?> GetFeeDetailByIdAsync(int feeDetailId)
        {
            return await _context.FeeDetails
                .Include(f => f.FeeType)
                .FirstOrDefaultAsync(f => f.FeeDetailId == feeDetailId);
        }

        public async Task<List<FeeDetail>> GetFeeDetailsByIdsAsync(List<int> feeDetailIds)
        {
            try
            {
                return await _context.FeeDetails
                    .Include(f => f.FeeType)
                    .Where(f => feeDetailIds.Contains(f.FeeDetailId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving fee details by IDs.", ex);
            }
        }



    }
}