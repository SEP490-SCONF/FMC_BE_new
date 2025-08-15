using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class ConferenceRoleRepository : IConferenceRoleRepository
    {
        private readonly ConferenceRoleDAO _conferenceRoleDAO;
        public ConferenceRoleRepository(ConferenceRoleDAO conferenceRoleDAO)
        {
            _conferenceRoleDAO = conferenceRoleDAO;
        }

        public async Task Add(ConferenceRole entity)
        {
           await _conferenceRoleDAO.AddConferenceRole(entity);
        }

        public async Task Delete(int id)
        {
            await _conferenceRoleDAO.DeleteConferenceRole(id);
        }

        public async Task<IEnumerable<ConferenceRole>> GetAll()
        {
           return await _conferenceRoleDAO.GetAllConferenceRoles();
        }

        public async Task<ConferenceRole> GetById(int id)
        {
            return await _conferenceRoleDAO.GetConferenceRoleById(id);
        }

        public async Task Update(ConferenceRole entity)
        {
             await _conferenceRoleDAO.UpdateConferenceRole(entity);
        }
        public async Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate)
        {
            return await _conferenceRoleDAO.GetByCondition(predicate);
        }
    }
}
