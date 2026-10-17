using System;

namespace IntelSphere.API.Models
{
    public class LeakedCredential
    {
        // Benzersiz kimlik (Primary Key)
        public int Id { get; set; }

        // Sızdırılan e-posta adresi (Örn: ahmet@beykent.edu.tr)
        public string Email { get; set; } = string.Empty;

        // E-postanın ait olduğu alan adı (Sorguları hızlandırmak için: beykent.edu.tr)
        public string Domain { get; set; } = string.Empty;

        // Sızan şifrenin SHA256 veya MD5 hash hali (Güvenlik için asla düz metin tutmayacağız)
        public string PasswordHash { get; set; } = string.Empty;

        // Bu verinin hangi sızıntı kaynağından geldiğini belirten ilişki (Foreign Key)
        public int BreachSourceId { get; set; }

        // İlişkisel Veri Tabanı Bağlantısı (Navigation Property)
        public BreachSource? BreachSource { get; set; }

        // Verinin sisteme işlendiği tarih
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    }
}