using System.ComponentModel.DataAnnotations;

namespace IntelSphere.API.DTOs
{
    public class MonitoredDomainCreateDto
    {
        [Required]
        [StringLength(253)]
        public string DomainName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string ContactEmail { get; set; } = string.Empty;
    }
}
