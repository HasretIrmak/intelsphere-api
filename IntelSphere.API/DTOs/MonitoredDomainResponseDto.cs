using System;

namespace IntelSphere.API.DTOs
{
    public class MonitoredDomainResponseDto
    {
        public int Id { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}