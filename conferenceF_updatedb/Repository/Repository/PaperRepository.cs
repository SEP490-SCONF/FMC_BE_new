using BussinessObject.Entity;
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

        public IQueryable<Paper> GetAllPapers()
        {
            return _paperDAO.GetAllPapers();
        }
        public async Task<Paper> GetPaperByIdAsync(int paperId)
        {
            return await _paperDAO.GetByIdAsync(paperId);
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
    }
}
