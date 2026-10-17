namespace IntelSphere.API.DTOs
{
    public class MonitoredDomainCreateDto
    {
        public string DomainName { get; set; } = string.Empty; // E.g.: beykent.edu.tr
        public string CompanyName { get; set; } = string.Empty; // E.g.: Beykent University
        public string ContactEmail { get; set; } = string.Empty; // E.g.: security@beykent.edu.tr
    }
}