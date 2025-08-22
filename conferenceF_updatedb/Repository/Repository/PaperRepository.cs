﻿using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class PaperRepository : IPaperRepository
    {
        private readonly PaperDAO _paperDAO; // Phụ thuộc trực tiếp vào PaperDAO

        public PaperRepository(PaperDAO paperDAO) // Inject PaperDAO
        {
            _paperDAO = paperDAO;
        }
        public async Task<List<Paper>> GetPapersByIdsAsync(List<int> paperIds)
        {
            // Gọi thẳng đến phương thức tương ứng của DAO
            return await _paperDAO.GetPapersByIdsAsync(paperIds);
        }
        public async Task<Paper?> GetPaperWithConferenceAndTimelinesAsync(int paperId)
        {
            return await _paperDAO.GetPaperWithConferenceAndTimelinesAsync(paperId);
        }
        public IQueryable<Paper> GetAllPapers()
        {
            return _paperDAO.GetAllPapers();
        }
        public async Task<List<PaperAuthor>> GetAuthorsByPaperIdAsync(int paperId)
        {
            return await _paperDAO.GetAuthorsByPaperIdAsync(paperId);
        }
        public async Task<Paper> GetPaperByIdAsync(int paperId)
        {
            return await _paperDAO.GetByIdAsync(paperId);
        }
        public async Task<Paper> GetPaperWithAuthorsAsync(int paperId)
        {
            return await _paperDAO.GetPaperWithAuthorsAsync(paperId);
        }
        public async Task<Paper?> GetPaperByIdWithIncludesAsync(int paperId)
        {
            return await _paperDAO.GetByIdWithIncludesAsync(paperId);
        }

        public async Task AddPaperAsync(Paper paper)
        {
            await _paperDAO.AddAsync(paper);
            await _paperDAO.SaveChangesAsync(); // Lưu thay đổi sau khi thêm
        }

        public async Task UpdatePaperAsync(Paper paper)
        {
            _paperDAO.Update(paper);
            await _paperDAO.SaveChangesAsync(); // Lưu thay đổi sau khi cập nhật
        }

        public async Task DeletePaperAsync(int paperId)
        {
            var paper = await _paperDAO.GetByIdAsync(paperId);
            if (paper != null)
            {
                _paperDAO.Delete(paper);
                await _paperDAO.SaveChangesAsync(); // Lưu thay đổi sau khi xóa
            }
        }

        public List<Paper> GetPapersByConferenceId(int conferenceId)
        {
            return _paperDAO.GetPapersByConferenceId(conferenceId);
        }
        public List<Paper> GetPapersByUserIdAndConferenceId(int userId, int conferenceId)
        {
            return _paperDAO.GetPapersByUserIdAndConferenceId(userId, conferenceId);
        }
        public List<Paper> GetPapersByConferenceIdAndStatus(int conferenceId, string status)
        {
            return _paperDAO.GetPapersByConferenceIdAndStatus(conferenceId, status);
        }
        public List<Paper> GetPublishedPapersByConferenceId(int conferenceId)
        {
            return _paperDAO.GetPublishedPapersByConferenceId(conferenceId);
        }
        public async Task<List<Paper>> GetAcceptedPapersWithRegistrationsByAuthor(int authorId)
        {
            return await _paperDAO.GetAcceptedPapersWithRegistrationsByAuthor(authorId);
        }
        public List<Paper> GetPapersByUserId(int userId)
        {
            return _paperDAO.GetPapersByUserId(userId);
        }


        public List<Paper> GetPresentedPapersByConferenceId(int conferenceId)
        {
            return _paperDAO.GetPresentedPapersByConferenceId(conferenceId);
        }





    }
}
