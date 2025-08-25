using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Certificates
{
    public class CertificateDto
    {
        public int CertificateId { get; set; }
        public int RegId { get; set; }
        public DateTime IssueDate { get; set; }
        public string? CertificateUrl { get; set; }
        public string? CertificateNumber { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserConferenceRoleId { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? ConferenceTitle { get; set; }
        public string? ConferenceRoleName { get; set; }
    }

    public class CertificateCreateDto
    {
        [Required]
        public int RegId { get; set; }
        public int? UserConferenceRoleId { get; set; }
    }

    public class CertificateUpdateDto
    {
        [Required]
        public int CertificateId { get; set; }
        public string? CertificateUrl { get; set; }
        public bool? Status { get; set; }
        public int? UserConferenceRoleId { get; set; }
    }

    public class CertificateGenerateDto
    {
        [Required]
        public int RegId { get; set; }
    }

    public class GenerateCertificateForAuthorDto
    {
        [Required]
        public int PaperId { get; set; }
        
        [Required]
        public int UserId { get; set; }
    }

    public class CertificateVerifyDto
    {
        public string CertificateNumber { get; set; } = string.Empty;
        public string BlockchainHash { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string? VerificationMessage { get; set; }
    }
}
