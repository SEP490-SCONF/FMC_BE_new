using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class CertificateDAO
    {
        private readonly ConferenceFTestContext _context;

        public CertificateDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Certificate>> GetAll()
        {
            try
            {
                return await _context.Certificates
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.User)
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.Conference)
                    .Include(c => c.UserConferenceRole)
                        .ThenInclude(ucr => ucr.ConferenceRole)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving certificates.", ex);
            }
        }

        public async Task<Certificate> GetById(int id)
        {
            try
            {
                return await _context.Certificates
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.User)
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.Conference)
                    .Include(c => c.UserConferenceRole)
                        .ThenInclude(ucr => ucr.ConferenceRole)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CertificateId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving certificate with ID {id}.", ex);
            }
        }

        public async Task<Certificate> GetByRegistrationId(int regId)
        {
            try
            {
                return await _context.Certificates
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.User)
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.Conference)
                    .Include(c => c.UserConferenceRole)
                        .ThenInclude(ucr => ucr.ConferenceRole)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.RegId == regId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving certificate for registration ID {regId}.", ex);
            }
        }

        public async Task<IEnumerable<Certificate>> GetByUserId(int userId)
        {
            try
            {
                return await _context.Certificates
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.User)
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.Conference)
                    .Include(c => c.UserConferenceRole)
                        .ThenInclude(ucr => ucr.ConferenceRole)
                    .Where(c => c.Reg.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving certificates for user ID {userId}.", ex);
            }
        }

        public async Task<IEnumerable<Certificate>> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.Certificates
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.User)
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.Conference)
                    .Include(c => c.UserConferenceRole)
                        .ThenInclude(ucr => ucr.ConferenceRole)
                    .Where(c => c.Reg.ConferenceId == conferenceId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving certificates for conference ID {conferenceId}.", ex);
            }
        }

        public async Task Add(Certificate certificate)
        {
            try
            {
                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding certificate.", ex);
            }
        }

        public async Task Update(Certificate certificate)
        {
            try
            {
                var existing = await _context.Certificates.FindAsync(certificate.CertificateId);
                if (existing == null)
                    throw new Exception($"Certificate with ID {certificate.CertificateId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(certificate);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating certificate with ID {certificate.CertificateId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var certificate = await _context.Certificates.FindAsync(id);
                if (certificate != null)
                {
                    _context.Certificates.Remove(certificate);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Certificate with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting certificate with ID {id}.", ex);
            }
        }

        public async Task<bool> ExistsByRegistrationId(int regId)
        {
            try
            {
                return await _context.Certificates.AnyAsync(c => c.RegId == regId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if certificate exists for registration ID {regId}.", ex);
            }
        }

        public async Task<bool> IsPaymentCompleted(int regId)
        {
            try
            {
                return await _context.Payments
                    .AnyAsync(p => p.RegId == regId && p.PayStatus == "PAID");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking payment status for registration ID {regId}.", ex);
            }
        }

        public async Task<bool> IsPaperApproved(int paperId)
        {
            try
            {
                return await _context.Papers
                    .AnyAsync(p => p.PaperId == paperId && (p.Status == "Approved" || p.Status == "Published"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking paper approval status for paper ID {paperId}.", ex);
            }
        }

        public async Task<IEnumerable<Certificate>> GetByPaperAuthor(int paperId, int authorId)
        {
            try
            {
                return await _context.Certificates
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.User)
                    .Include(c => c.Reg)
                        .ThenInclude(r => r.Conference)
                    .Include(c => c.UserConferenceRole)
                    .Where(c => c.Reg.UserId == authorId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving certificates for paper {paperId} and author {authorId}.", ex);
            }
        }

        public async Task<IEnumerable<int>> GetApprovedPaperAuthors(int paperId)
        {
            try
            {
                return await _context.PaperAuthors
                    .Where(pa => pa.PaperId == paperId)
                    .Join(_context.Papers,
                        pa => pa.PaperId,
                        p => p.PaperId,
                        (pa, p) => new { PaperAuthor = pa, Paper = p })
                    .Where(joined => joined.Paper.Status == "Approved" || joined.Paper.Status == "Published")
                    .Select(joined => joined.PaperAuthor.AuthorId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving approved paper authors for paper ID {paperId}.", ex);
            }
        }
    }
}
