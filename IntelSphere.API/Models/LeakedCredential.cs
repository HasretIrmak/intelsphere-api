using System;

namespace IntelSphere.API.Models
{
    public class LeakedCredential
    {
        // Unique identifier (Primary Key)
        public int Id { get; set; }

        // Leaked email address (E.g., ahmet@beykent.edu.tr)
        public string Email { get; set; } = string.Empty;

        // Domain name to which the email belongs (To speed up queries: beykent.edu.tr)
        public string Domain { get; set; } = string.Empty;

        // SHA256 or MD5 hash of the leaked password (We will never store plain text for security)
        public string PasswordHash { get; set; } = string.Empty;

        // Foreign Key indicating which breach source this data came from
        public int BreachSourceId { get; set; }

        // Relational Database Connection (Navigation Property)
        public BreachSource? BreachSource { get; set; }

        // Date when the data was processed into the system
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    }
}