using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ConferenceDAO
    {
        private readonly ConferenceFTestContext _context;

        public ConferenceDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all conferences
        public async Task<IEnumerable<Conference>> GetAllConferences()
        {
            try
            {
                return await _context.Conferences
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all conferences.", ex);
            }
        }

        // Get conference by ID
        public async Task<Conference> GetConferenceById(int id)
        {
            try
            {
                return await _context.Conferences
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.ConferenceId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving conference with ID {id}.", ex);
            }
        }

        // Add a new conference
        public async Task AddConference(Conference conference)
        {
            try
            {
                _context.Conferences.Add(conference);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding a new conference.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding a new conference.", ex);
            }
        }

        // Update existing conference
        public async Task UpdateConference(Conference conference)
        {
            try
            {
                var existing = await GetConferenceById(conference.ConferenceId);
                if (existing == null)
                    throw new Exception($"Conference with ID {conference.ConferenceId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(conference);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while updating the conference.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating conference with ID {conference.ConferenceId}.", ex);
            }
        }

        // Delete conference by ID
        public async Task DeleteConference(int id)
        {
            try
            {
                var conference = await GetConferenceById(id);
                if (conference != null)
                {
                    _context.Conferences.Remove(conference);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Conference with ID {id} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting the conference.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting conference with ID {id}.", ex);
            }
        }

        // Count conferences
        public async Task<int> GetConferenceCount()
        {
            try
            {
                return await _context.Conferences.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while counting conferences.", ex);
            }
        }
        public async Task UpdateConferenceStatus(int conferenceId, string newStatus)
        {
            try
            {
                var conference = await _context.Conferences.FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
                if (conference == null)
                    throw new Exception($"Conference with ID {conferenceId} not found.");

                conference.Status = true;
                _context.Conferences.Update(conference);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while updating the conference status.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating status for conference with ID {conferenceId}.", ex);
            }
        }
    }
}
