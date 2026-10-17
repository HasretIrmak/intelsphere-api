using System;

namespace IntelSphere.API.DTOs
{
    public class BreachSourceCreateDto
    {
        // Required fields to be entered by the user
        public string SourceName { get; set; } = string.Empty;
        public DateTime BreachDate { get; set; }
        public string Severity { get; set; } = "Medium"; // Default is Medium
    }
}