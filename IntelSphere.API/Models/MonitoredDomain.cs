using System;

namespace IntelSphere.API.Models
{
    public class MonitoredDomain
    {
        // Unique identifier (Primary Key)
        public int Id { get; set; }

        // Monitored company domain name (E.g., beykent.edu.tr)
        public string DomainName { get; set; } = string.Empty;

        // Name or title of the company
        public string CompanyName { get; set; } = string.Empty;

        // Corporate email address where notifications will be sent
        public string ContactEmail { get; set; } = string.Empty;

        // The latest calculated cyber risk score for the company (Between 0 and 100)
        public int RiskScore { get; set; } = 0;

        // Date when monitoring was initiated
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}