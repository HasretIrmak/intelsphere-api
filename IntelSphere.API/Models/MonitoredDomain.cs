using System;

namespace IntelSphere.API.Models
{
    public class MonitoredDomain
    {
        // Benzersiz kimlik (Primary Key)
        public int Id { get; set; }

        // Takip edilen şirket alan adı (Örn: beykent.edu.tr)
        public string DomainName { get; set; } = string.Empty;

        // Şirketin adı veya unvanı
        public string CompanyName { get; set; } = string.Empty;

        // Bildirimlerin gönderileceği kurumsal e-posta
        public string ContactEmail { get; set; } = string.Empty;

        // Şirket için hesaplanan en güncel siber risk skoru (0 - 100 arası)
        public int RiskScore { get; set; } = 0;

        // Takibin başlatıldığı tarih
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}