﻿using ConferenceFWebAPI.DTOs.PaperRevisions;

namespace ConferenceFWebAPI.DTOs.Papers
{
    public class PaperResponseWT
    {
        public int PaperId { get; set; } // Giữ lại ID để xác định bài báo

        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public string? Name { get; set; }
        public string TopicName { get; set; }
        public bool IsAssigned { get; set; } // Đã có reviewer hay chưa
        public string? AssignedReviewerName { get; set; } // Tên reviewer đầu tiên (nếu có)
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmitDate { get; set; }
        public int? AssignmentId { get; set; }
        // Thêm một thuộc tính để chứa thông tin từ PaperRevision
        public List<PaperRevisionDTO> PaperRevisions { get; set; } // Dữ liệu của PaperRevision
    }

    // Tạo DTO cho PaperRevision
   
}
