using System;
using System.ComponentModel.DataAnnotations;

namespace IntelSphere.API.DTOs
{
    public class BreachSourceCreateDto
    {
        // Required fields to be entered by the user
        [Required]
        [StringLength(200)]
        public string SourceName { get; set; } = string.Empty;

        public DateTime BreachDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Default is Medium
    }
}
