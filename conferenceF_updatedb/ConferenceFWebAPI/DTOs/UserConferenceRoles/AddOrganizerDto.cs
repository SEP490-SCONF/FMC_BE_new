namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class AddOrganizerDto
    {
        public string Email { get; set; } 
        public int ConferenceId { get; set; } 
        public string SpecificTitle { get; set; }
    }
}
