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
                                     .Where(c => c.Status == true)
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all active conferences.", ex);
            }
        }
        public async Task<IEnumerable<Conference>> GetAllConferencesFalse()
        {
            try
            {
                return await _context.Conferences
                                     .Where(c => c.Status == false)
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all active conferences.", ex);
            }
        }


        // Get conference by ID (include Topics)
        public async Task<Conference> GetConferenceById(int id)
        {
            try
            {
                return await _context.Conferences
                                     .Include(c => c.Topics) 
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
        public async Task UpdateConference(Conference updatedConference)
        {
            var existingConference = await _context.Conferences
                .Include(c => c.Topics) // ✅ Bao gồm danh sách topic hiện tại
                .FirstOrDefaultAsync(c => c.ConferenceId == updatedConference.ConferenceId);

            if (existingConference == null)
            {
                throw new Exception($"Conference with ID {updatedConference.ConferenceId} not found.");
            }

            // ✅ Cập nhật các thuộc tính scalar
            _context.Entry(existingConference).CurrentValues.SetValues(updatedConference);

            // ✅ Cập nhật lại các Topic mới nếu có
            if (updatedConference.Topics != null && updatedConference.Topics.Any())
            {
                // Xóa liên kết cũ
                existingConference.Topics.Clear();

                // Gán lại danh sách Topic mới
                foreach (var topic in updatedConference.Topics)
                {
                    var trackedTopic = await _context.Topics.FindAsync(topic.TopicId);
                    if (trackedTopic != null)
                    {
                        existingConference.Topics.Add(trackedTopic);
                    }
                }
            }

            await _context.SaveChangesAsync();
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
        public async Task UpdateConferenceStatus(int conferenceId, bool newStatus)
        {
            try
            {
                var conference = await _context.Conferences.FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
                if (conference == null)
                    throw new Exception($"Conference with ID {conferenceId} not found.");

                conference.Status = newStatus;
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
