using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ScheduleDAO
    {
        private readonly ConferenceFTestContext _context;

        public ScheduleDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all schedules (basic)
        public async Task<IEnumerable<Schedule>> GetAllSchedules()
        {
            try
            {
                return await _context.Schedules
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all schedules.", ex);
            }
        }

        // Get schedule by ID (basic)
        public async Task<Schedule> GetScheduleById(int id)
        {
            try
            {
                return await _context.Schedules
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(s => s.ScheduleId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving schedule with ID {id}.", ex);
            }
        }

        // Add a new schedule
        public async Task AddSchedule(Schedule schedule)
        {
            try
            {
                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding a new schedule.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding a new schedule.", ex);
            }
        }

        // Update existing schedule
        public async Task UpdateSchedule(Schedule schedule)
        {
            try
            {
                var existing = await _context.Schedules.FindAsync(schedule.ScheduleId);
                if (existing == null)
                    throw new Exception($"Schedule with ID {schedule.ScheduleId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(schedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while updating the schedule.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating schedule with ID {schedule.ScheduleId}.", ex);
            }
        }

        // Delete schedule by ID
        public async Task DeleteSchedule(int id)
        {
            try
            {
                var schedule = await _context.Schedules.FindAsync(id);
                if (schedule != null)
                {
                    _context.Schedules.Remove(schedule);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Schedule with ID {id} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting the schedule.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting schedule with ID {id}.", ex);
            }
        }

        // Get schedules by conference ID (basic)
        public async Task<IEnumerable<Schedule>> GetSchedulesByConference(int conferenceId)
        {
            try
            {
                return await _context.Schedules
                                     .Where(s => s.ConferenceId == conferenceId)
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving schedules for conference ID {conferenceId}.", ex);
            }
        }
    }
}
