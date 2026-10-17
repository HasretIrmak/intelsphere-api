using System;

namespace IntelSphere.API.Models
{
    public class BreachSource
    {
        // Benzersiz kimlik (Primary Key)
        public int Id { get; set; }

        // Sızıntının adı (Örn: LinkedIn Veri Sızıntısı)
        public string SourceName { get; set; } = string.Empty;

        // Verinin sızdırıldığı tahmini tarih
        public DateTime BreachDate { get; set; }

        // Sisteme eklenme tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Sızıntının tehlike seviyesi (Örn: Low, Medium, High, Critical)
        public string Severity { get; set; } = "Medium";
    }
}