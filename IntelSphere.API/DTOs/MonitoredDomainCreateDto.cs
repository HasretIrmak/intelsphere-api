namespace IntelSphere.API.DTOs
{
    public class MonitoredDomainCreateDto
    {
        public string DomainName { get; set; } = string.Empty; // Örn: beykent.edu.tr
        public string CompanyName { get; set; } = string.Empty; // Örn: Beykent Üniversitesi
        public string ContactEmail { get; set; } = string.Empty; // Örn: security@beykent.edu.tr
    }
}