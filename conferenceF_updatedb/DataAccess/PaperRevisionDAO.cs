using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PaperRevisionDAO
    {
        private readonly ConferenceFTestContext _context;

        public PaperRevisionDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public List<PaperRevision> GetPaperRevisionsByPaperId(int paperId)
        {
            return _context.PaperRevisions
                           .Where(pr => pr.PaperId == paperId)
                           .ToList();
        }

        public async Task<PaperRevision?> GetPaperRevisionByIdAsync(int revisionId)
        {
            return await _context.PaperRevisions
                                 .Include(pr => pr.Paper)
                                 .FirstOrDefaultAsync(pr => pr.RevisionId == revisionId);
        }

        public async Task<PaperRevision> AddPaperRevisionAsync(PaperRevision paperRevision)
        {
            await _context.PaperRevisions.AddAsync(paperRevision);
            await _context.SaveChangesAsync();
            return paperRevision;
        }

        public async Task UpdatePaperRevisionAsync(PaperRevision paperRevision)
        {
            _context.Entry(paperRevision).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaperRevisionAsync(int revisionId)
        {
            var paperRevisionToDelete = await _context.PaperRevisions.FindAsync(revisionId);
            if (paperRevisionToDelete != null)
            {
                _context.PaperRevisions.Remove(paperRevisionToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PaperRevision>> GetRevisionsByPaperIdAsync(int paperId)
        {
            return await _context.PaperRevisions
                                 .Where(pr => pr.PaperId == paperId)
                                 .OrderByDescending(pr => pr.SubmittedAt)
                                 .ToListAsync();
        }
    }

}
