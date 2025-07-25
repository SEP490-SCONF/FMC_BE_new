using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PaperDAO
    {
        private readonly ConferenceFTestContext _context;

        public PaperDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<Paper?> GetPaperWithConferenceAndTimelinesAsync(int paperId)
        {
            return await _context.Papers
                                 .Include(p => p.Conference)
                                     .ThenInclude(c => c.TimeLines)
                                 .FirstOrDefaultAsync(p => p.PaperId == paperId);
        }
        public List<Paper> GetPapersByConferenceId(int conferenceId)
        {
            return _context.Papers
                           .Where(p => p.ConferenceId == conferenceId)
                           .ToList();
        }
        public IQueryable<Paper> GetAllPapers()
        {
            return _context.Papers.Where(p => p.Status != "Deleted").AsQueryable();
        }

        public async Task<Paper> GetByIdAsync(int id)
        {
            return await _context.Papers.FindAsync(id);
        }

        public async Task AddAsync(Paper entity)
        {
            await _context.Papers.AddAsync(entity);
        }

        public void Update(Paper entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(Paper entity)
        {
            _context.Papers.Remove(entity);
        }
        public List<Paper> GetPapersByUserIdAndConferenceId(int userId, int conferenceId)
        {
            return _context.Papers
                           .Where(p => p.ConferenceId == conferenceId &&
                                       p.PaperAuthors.Any(pa => pa.AuthorId == userId))
                           .Include(p => p.Topic)
                            .Include(p => p.PaperAuthors)
                            .ThenInclude(pa => pa.Author)
                            .Include(p => p.PaperRevisions)
                           .ToList();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public List<Paper> GetPapersByConferenceIdAndStatus(int conferenceId, string status)
        {
            return _context.Papers
                .Where(p => p.ConferenceId == conferenceId && p.Status == status)
                .Include(p => p.Topic)
                .Include(p => p.PaperAuthors)
                    .ThenInclude(pa => pa.Author)
                .Include(p => p.ReviewerAssignments)
                    .ThenInclude(ra => ra.Reviewer)
                        .ThenInclude(r => r.UserConferenceRoles)
                            .ThenInclude(ucr => ucr.ConferenceRole)
                .ToList();
        }


    }
}

