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

        public async Task<IEnumerable<PaperRevision>> GetAll()
        {
            try
            {
                return await _context.PaperRevisions.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving paper revisions.", ex);
            }
        }

        public async Task<PaperRevision> GetById(int id)
        {
            try
            {
                return await _context.PaperRevisions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.RevisionId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paper revision with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<PaperRevision>> GetByPaperId(int paperId)
        {
            try
            {
                return await _context.PaperRevisions
                    .Where(p => p.PaperId == paperId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving revisions for paper ID {paperId}.", ex);
            }
        }

        public async Task Add(PaperRevision revision)
        {
            try
            {
                _context.PaperRevisions.Add(revision);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding paper revision.", ex);
            }
        }

        public async Task Update(PaperRevision revision)
        {
            try
            {
                var existing = await _context.PaperRevisions.FindAsync(revision.RevisionId);
                if (existing == null)
                    throw new Exception($"PaperRevision with ID {revision.RevisionId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(revision);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating paper revision with ID {revision.RevisionId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var revision = await _context.PaperRevisions.FindAsync(id);
                if (revision != null)
                {
                    _context.PaperRevisions.Remove(revision);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Paper revision with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting paper revision with ID {id}.", ex);
            }
        }
    }

}
