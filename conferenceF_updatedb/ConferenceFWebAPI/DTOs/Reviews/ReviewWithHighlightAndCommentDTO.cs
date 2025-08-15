using ConferenceFWebAPI.DTOs.Paper;
using System;
using System.Collections.Generic;

namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class ReviewWithHighlightAndCommentDTO
    {
        // Review
        public int ReviewId { get; set; }
        public int PaperId { get; set; }
        public int ReviewerId { get; set; }
        public int? RevisionId { get; set; }
        public int? Score { get; set; }
        public string? Comment { get; set; }
        public string? Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RevisionStatus { get; set; }
        public string? FilePath { get; set; }
        public PaperResponseDto? Paper { get; set; }

        // List of Highlights
        public List<HighlightDTO> Highlights { get; set; } = new();

        // List of Comments
        public List<CommentsDTO> Comments { get; set; } = new();
    }

    public class HighlightDTO
    {
        public int HighlightId { get; set; }
        public string? TextHighlighted { get; set; }
        public List<HighlightAreaDTO> Areas { get; set; } = new();
    }

    public class CommentsDTO
    {
        public int CommentId { get; set; }
        public int HighlightId { get; set; }
        public int UserId { get; set; }
        public string? CommentText { get; set; }
        public string? CommentStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

