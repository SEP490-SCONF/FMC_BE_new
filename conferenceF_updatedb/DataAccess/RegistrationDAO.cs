using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class RegistrationDAO
    {
        private readonly ConferenceFTestContext _context;

        public RegistrationDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Registration>> GetAll()
        {
            try
            {
                return await _context.Registrations.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving registrations.", ex);
            }
        }

        public async Task<Registration> GetById(int id)
        {
            try
            {
                return await _context.Registrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RegId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving registration with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Registration>> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.Registrations
                    .Where(r => r.ConferenceId == conferenceId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving registrations for conference ID {conferenceId}.", ex);
            }
        }

        public async Task<IEnumerable<Registration>> GetByUserId(int userId)
        {
            try
            {
                return await _context.Registrations
                    .Where(r => r.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving registrations for user ID {userId}.", ex);
            }
        }

        public async Task Add(Registration registration)
        {
            try
            {
                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding registration.", ex);
            }
        }

        public async Task Update(Registration registration)
        {
            try
            {
                var existing = await _context.Registrations.FindAsync(registration.RegId);
                if (existing == null)
                    throw new Exception($"Registration with ID {registration.RegId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(registration);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating registration with ID {registration.RegId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var registration = await _context.Registrations.FindAsync(id);
                if (registration != null)
                {
                    _context.Registrations.Remove(registration);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Registration with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting registration with ID {id}.", ex);
            }
        }
    }

}
