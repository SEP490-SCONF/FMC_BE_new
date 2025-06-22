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
                    .Where(p => p.UserId == userId)
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
    }
}
