namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class CompleteCommitteeDto
    {
        public string Token { get; set; }
        public string GroupName { get; set; }
        public string SpecificTitle { get; set; }
        public string Affiliation { get; set; }
        public string Expertise { get; set; }
        public string DisplayNameOverride { get; set; }
    }
}
