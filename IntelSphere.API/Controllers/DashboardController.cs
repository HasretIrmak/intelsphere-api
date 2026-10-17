using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelSphere.API.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IntelSphere.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET CENTRAL INTELLIGENCE OVERVIEW -> GET: api/v1/Dashboard/overview
        [HttpGet("overview")]
        public async Task<IActionResult> GetIntelligenceOverview()
        {
            // 1. Basic Counters
            var totalMonitoredCompanies = await _context.MonitoredDomains.CountAsync();
            var totalBreachSources = await _context.BreachSources.CountAsync();
            var totalLeakedCredentials = await _context.LeakedCredentials.CountAsync();

            // 2. Average Risk Score of Monitored Companies
            double averageRiskScore = 0;
            if (totalMonitoredCompanies > 0)
            {
                averageRiskScore = await _context.MonitoredDomains.AverageAsync(d => d.RiskScore);
            }

            // 3. Top 3 Riskiest Companies (Critical Reporting)
            var topRiskyCompanies = await _context.MonitoredDomains
                .OrderByDescending(d => d.RiskScore)
                .Take(3)
                .Select(d => new
                {
                    d.CompanyName,
                    d.DomainName,
                    d.RiskScore,
                    RiskLevel = d.RiskScore >= 75 ? "Critical" : d.RiskScore >= 50 ? "High" : "Medium/Low"
                })
                .ToListAsync();

            // 4. General Cyber Threat Level Determination
            string generalThreatLevel = "MEDIUM LEVEL CYBER ACTIVITY";
            if (averageRiskScore >= 75) generalThreatLevel = "CRITICAL CONDITION (RED ALERT)";
            else if (averageRiskScore >= 50) generalThreatLevel = "HIGH THREAT (ORANGE ALERT)";
            else if (averageRiskScore == 0 && totalLeakedCredentials == 0) generalThreatLevel = "LOW LEVEL / SECURE";

            // Combining all analytics into a single report and returning it
            return Ok(new
            {
                reportDate = System.DateTime.UtcNow,
                generalCyberThreatLevel = generalThreatLevel,
                systemGeneralStatistics = new
                {
                    totalMonitoredDomains = totalMonitoredCompanies,
                    totalBreachSources = totalBreachSources,
                    totalLeakedCredentialsInDatabase = totalLeakedCredentials,
                    monitoredCompaniesAverageRiskScore = System.Math.Round(averageRiskScore, 2)
                },
                topRiskyCompanies = topRiskyCompanies
            });
        }
    }
}