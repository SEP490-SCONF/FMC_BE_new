﻿using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DataAccess
{
    public class ReviewerAssignmentDAO
    {
        private readonly ConferenceFTestContext _context;

        public ReviewerAssignmentDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetAll()
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Include(r => r.Paper)
                        .ThenInclude(p => p.PaperRevisions)
                    .Include(r => r.Paper.Topic) 
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving reviewer assignments.", ex);
            }
        }

        public async Task<List<ReviewerAssignment>> GetReviewersByPaperIdAsync(int paperId)
        {
            return await _context.ReviewerAssignments
                .Where(ra => ra.PaperId == paperId)
                .Include(ra => ra.Reviewer)
                .ToListAsync();
        }
        public async Task<ReviewerAssignment> GetById(int id)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Include(r => r.Paper)
                        .ThenInclude(p => p.PaperRevisions.Where(pr => pr.Status == "Under Review"))
                    .Include(r => r.Paper.Topic) 
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.AssignmentId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reviewer assignment with ID {id}.", ex);
            }
        }




        public async Task<IEnumerable<ReviewerAssignment>> GetByPaperId(int paperId)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Include(r => r.Paper)
                        .ThenInclude(p => p.PaperRevisions.Where(rev => rev.Status == "Under Review"))
                    .Include(r => r.Paper.Topic) 
                    .Where(r => r.PaperId == paperId &&
                                r.Paper.PaperRevisions.Any(rev => rev.Status == "Under Review"))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reviewer assignments for paper ID {paperId}.", ex);
            }
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetAllByPaperId(int paperId)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Include(r => r.Paper)
                        .ThenInclude(p => p.PaperRevisions)
                    .Include(r => r.Paper.Topic)
                    .Where(r => r.PaperId == paperId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all reviewer assignments for paper ID {paperId}.", ex);
            }
        }

        public async Task Add(ReviewerAssignment entity)
        {
            try
            {
                _context.ReviewerAssignments.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding reviewer assignment.", ex);
            }
        }

        public async Task Update(ReviewerAssignment entity)
        {
            try
            {
                var existing = await _context.ReviewerAssignments.FindAsync(entity.AssignmentId);
                if (existing == null)
                    throw new Exception($"ReviewerAssignment with ID {entity.AssignmentId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating reviewer assignment with ID {entity.AssignmentId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var entity = await _context.ReviewerAssignments.FindAsync(id);
                if (entity != null)
                {
                    _context.ReviewerAssignments.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"ReviewerAssignment with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting reviewer assignment with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetByReviewerId(int reviewerId)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Include(r => r.Paper)
                        .ThenInclude(p => p.PaperRevisions)
                    .Include(r => r.Paper.Topic) 
                    .Where(r => r.ReviewerId == reviewerId )
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving reviewer assignments for reviewer ID {reviewerId}.", ex);
            }
        }
        public async Task<int> GetAssignedPaperCountByReviewerIdAndConferenceId(int reviewerId, int conferenceId)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Where(r => r.ReviewerId == reviewerId && r.Paper.ConferenceId == conferenceId)
                    .Select(r => r.PaperId)
                    .Distinct() // chỉ tính 1 lần mỗi paper
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assigned paper count for reviewer ID {reviewerId} in conference {conferenceId}.", ex);
            }
        }
        public async Task<int> GetAssignmentCountByReviewerIdAndConferenceId(int reviewerId, int conferenceId)
        {
            try
            {
                return await _context.ReviewerAssignments
                    .Where(r => r.ReviewerId == reviewerId && r.Paper.ConferenceId == conferenceId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assignment count for reviewer ID {reviewerId} in conference {conferenceId}.", ex);
            }
        }


    }
}
