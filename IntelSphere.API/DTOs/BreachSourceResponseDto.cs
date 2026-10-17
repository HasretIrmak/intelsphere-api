using System;

namespace IntelSphere.API.DTOs
{
    public class BreachSourceResponseDto
    {
        public int Id { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public DateTime BreachDate { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}