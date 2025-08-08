using ConferenceFWebAPI.DTOs.Conferences;

namespace ConferenceFWebAPI.DTOs.CallForPapers
{
    public class CallForPaperDto
    {
        public int Cfpid { get; set; }
        public int ConferenceId { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public bool Status { get; set; }

        public string? TemplatePath { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public ConferenceResponseDTO? Conference { get; set; }

    }
}
