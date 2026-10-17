using System;

namespace IntelSphere.API.Models
{
    public class BreachSource
    {
        // Unique identifier (Primary Key)
        public int Id { get; set; }

        // Name of the breach (E.g., LinkedIn Data Breach)
        public string SourceName { get; set; } = string.Empty;

        // Estimated date when the data was breached
        public DateTime BreachDate { get; set; }

        // Date added to the system
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Severity level of the breach (E.g., Low, Medium, High, Critical)
        public string Severity { get; set; } = "Medium";
    }
}