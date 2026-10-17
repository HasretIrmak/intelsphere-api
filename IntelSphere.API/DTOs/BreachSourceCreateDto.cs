using System;

namespace IntelSphere.API.DTOs
{
    public class BreachSourceCreateDto
    {
        // Kullanıcının girmesi zorunlu alanlar
        public string SourceName { get; set; } = string.Empty;
        public DateTime BreachDate { get; set; }
        public string Severity { get; set; } = "Medium"; // Varsayılan olarak Medium
    }
}