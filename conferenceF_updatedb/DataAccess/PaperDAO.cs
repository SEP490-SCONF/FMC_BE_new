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

        /// <summary>
        /// Lấy tất cả các bài báo, bao gồm thông tin chi tiết về các tác giả.
        /// </summary>
        public async Task<List<Paper>> GetAllAsync()
        {
            // Sử dụng .Include() để tải các PaperAuthor và .ThenInclude() để tải thông tin User (tác giả) liên quan.
            // Điều này giúp tránh lỗi N+1 query và lấy đủ dữ liệu cần thiết trong một lần gọi.
            return await _context.Papers
                .Include(p => p.PaperAuthors)
                    .ThenInclude(pa => pa.Author) // Giả định PaperAuthor có navigation property tên là 'Author' đến User
                .ToListAsync();
        }

        /// <summary>
        /// Lấy một bài báo theo ID, bao gồm thông tin chi tiết về các tác giả.
        /// </summary>
        public async Task<Paper?> GetByIdAsync(int id)
        {
            return await _context.Papers
                .Include(p => p.PaperAuthors)
                    .ThenInclude(pa => pa.Author)
                .FirstOrDefaultAsync(p => p.PaperId == id);
        }

        /// <summary>
        /// Thêm một bài báo mới cùng với danh sách các tác giả.
        /// </summary>
        /// <param name="paper">Đối tượng bài báo cần thêm.</param>
        /// <param name="authorIds">Danh sách các ID của tác giả.</param>
        public async Task AddAsync(Paper paper, List<int> authorIds)
        {
            if (authorIds == null || !authorIds.Any())
            {
                throw new ArgumentException("A paper must have at least one author.", nameof(authorIds));
            }

            // Xóa hết PaperAuthors cũ nếu có (tránh trường hợp client gửi kèm)
            paper.PaperAuthors.Clear();

            // Tạo các thực thể liên kết PaperAuthor từ danh sách authorIds
            int order = 1;
            foreach (var authorId in authorIds)
            {
                var paperAuthor = new PaperAuthor
                {
                    AuthorId = authorId,
                    AuthorOrder = order++
                };
                paper.PaperAuthors.Add(paperAuthor);
            }

            _context.Papers.Add(paper);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật thông tin một bài báo và danh sách tác giả của nó.
        /// </summary>
        /// <param name="paper">Đối tượng bài báo với thông tin đã cập nhật.</param>
        /// <param name="authorIds">Danh sách ID tác giả mới.</param>
        public async Task UpdateAsync(Paper paper, List<int> authorIds)
        {
            // Tìm bài báo hiện có trong database, bao gồm cả các tác giả hiện tại.
            var existingPaper = await _context.Papers
                                              .Include(p => p.PaperAuthors)
                                              .FirstOrDefaultAsync(p => p.PaperId == paper.PaperId);

            if (existingPaper != null)
            {
                // Cập nhật các thuộc tính của bài báo (Title, Abstract, etc.)
                _context.Entry(existingPaper).CurrentValues.SetValues(paper);

                // Cập nhật danh sách tác giả
                // 1. Xóa các tác giả không còn trong danh sách mới
                var authorsToRemove = existingPaper.PaperAuthors
                                                   .Where(pa => !authorIds.Contains(pa.AuthorId))
                                                   .ToList();
                if (authorsToRemove.Any())
                {
                    _context.PaperAuthors.RemoveRange(authorsToRemove);
                }

                // 2. Thêm các tác giả mới (chưa có trong danh sách cũ)
                int order = 1;
                var existingAuthorIds = existingPaper.PaperAuthors.Select(pa => pa.AuthorId).ToList();
                foreach (var authorId in authorIds)
                {
                    if (!existingAuthorIds.Contains(authorId))
                    {
                        var newPaperAuthor = new PaperAuthor
                        {
                            PaperId = existingPaper.PaperId,
                            AuthorId = authorId,
                            AuthorOrder = order
                        };
                        _context.PaperAuthors.Add(newPaperAuthor);
                    }
                    order++;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Paper with Id {paper.PaperId} not found.");
            }
        }

        /// <summary>
        /// Cập nhật chỉ các thuộc tính đơn giản của Paper (không bao gồm collection).
        /// </summary>
        public async Task UpdateSimpleAsync(Paper paper)
        {
            // Chỉ cập nhật các trường của đối tượng Paper.
            // Phương thức này không thay đổi danh sách tác giả hoặc các collection khác.
            _context.Entry(paper).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Xóa một bài báo theo ID.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var paper = await _context.Papers.FindAsync(id);
            if (paper != null)
            {
                // Khi một Paper bị xóa, EF Core sẽ tự động xóa các PaperAuthor liên quan
                // do mối quan hệ 1-nhiều đã được định nghĩa.
                _context.Papers.Remove(paper);
                await _context.SaveChangesAsync();
            }
        }
    }
}

