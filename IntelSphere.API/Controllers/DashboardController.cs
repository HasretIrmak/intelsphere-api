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
            // 1. Temel Sayaçlar
            var totalMonitoredCompanies = await _context.MonitoredDomains.CountAsync();
            var totalBreachSources = await _context.BreachSources.CountAsync();
            var totalLeakedCredentials = await _context.LeakedCredentials.CountAsync();

            // 2. Takipteki Şirketlerin Ortalama Risk Skoru
            double averageRiskScore = 0;
            if (totalMonitoredCompanies > 0)
            {
                averageRiskScore = await _context.MonitoredDomains.AverageAsync(d => d.RiskScore);
            }

            // 3. En Riskli İlk 3 Şirket (Kritik Raporlama)
            var topRiskiveCompanies = await _context.MonitoredDomains
                .OrderByDescending(d => d.RiskScore)
                .Take(3)
                .Select(d => new
                {
                    d.CompanyName,
                    d.DomainName,
                    d.RiskScore,
                    RiskSeviyesi = d.RiskScore >= 75 ? "Kritik" : d.RiskScore >= 50 ? "Yüksek" : "Orta/Düşük"
                })
                .ToListAsync();

            // 4. Genel Siber Tehdit Seviyesi Belirleme
            string generalThreatLevel = "ORTA SEVİYE SİBER AKTİVİTE";
            if (averageRiskScore >= 75) generalThreatLevel = "KRİTİK DURUM (KIRMIZI ALARM)";
            else if (averageRiskScore >= 50) generalThreatLevel = "YÜKSEK TEHDİT (TURUNCU ALARM)";
            else if (averageRiskScore == 0 && totalLeakedCredentials == 0) generalThreatLevel = "DÜŞÜK SEVİYE / GÜVENLİ";

            // Tüm analitiği tek bir raporda birleştirip dönüyoruz
            return Ok(new
            {
                raporTarihi = System.DateTime.UtcNow,
                genelSiberTehditSeviyesi = generalThreatLevel,
                sistemGenelIstatistikleri = new
                {
                    toplamTakipEdilenDomain = totalMonitoredCompanies,
                    toplamSiziantiKaynakPaketi = totalBreachSources,
                    veriTabanindakiToplamSiziantiHesap = totalLeakedCredentials,
                    izlenenKurumlarRiskOrtalamasi = System.Math.Round(averageRiskScore, 2)
                },
                enYuksekRiskliKurumlar = topRiskiveCompanies
            });
        }
    }
}