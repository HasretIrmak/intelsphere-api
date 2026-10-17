using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelSphere.API.Data;
using IntelSphere.API.Models;
using IntelSphere.API.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelSphere.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MonitoredDomainsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MonitoredDomainsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. READ ALL -> GET: api/v1/MonitoredDomains
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoredDomainResponseDto>>> GetDomains()
        {
            var domains = await _context.MonitoredDomains.ToListAsync();

            var response = domains.Select(d => new MonitoredDomainResponseDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                CompanyName = d.CompanyName,
                ContactEmail = d.ContactEmail,
                RiskScore = d.RiskScore,
                CreatedAt = d.CreatedAt
            }).ToList();

            return Ok(response);
        }

        // 2. READ BY ID -> GET: api/v1/MonitoredDomains/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoredDomainResponseDto>> GetDomain(int id)
        {
            var domain = await _context.MonitoredDomains.FindAsync(id);

            if (domain == null)
            {
                return NotFound(new { message = $"{id} ID'li izlenen domain bulunamadı." });
            }

            var response = new MonitoredDomainResponseDto
            {
                Id = domain.Id,
                DomainName = domain.DomainName,
                CompanyName = domain.CompanyName,
                ContactEmail = domain.ContactEmail,
                RiskScore = domain.RiskScore,
                CreatedAt = domain.CreatedAt
            };

            return Ok(response);
        }

        // 3. CREATE -> POST: api/v1/MonitoredDomains
        [HttpPost]
        public async Task<ActionResult<MonitoredDomainResponseDto>> CreateDomain([FromBody] MonitoredDomainCreateDto createDto)
        {
            var newDomain = new MonitoredDomain
            {
                DomainName = createDto.DomainName.ToLower().Trim(), // Domain standart küçük harf ve temiz olmalı
                CompanyName = createDto.CompanyName,
                ContactEmail = createDto.ContactEmail,
                RiskScore = 0 // Yeni eklenen şirketin risk skoru başlangıçta 0'dır.
            };

            _context.MonitoredDomains.Add(newDomain);
            await _context.SaveChangesAsync();

            var response = new MonitoredDomainResponseDto
            {
                Id = newDomain.Id,
                DomainName = newDomain.DomainName,
                CompanyName = newDomain.CompanyName,
                ContactEmail = newDomain.ContactEmail,
                RiskScore = newDomain.RiskScore,
                CreatedAt = newDomain.CreatedAt
            };

            return CreatedAtAction(nameof(GetDomain), new { id = response.Id }, response);
        }

        // 4. UPDATE -> PUT: api/v1/MonitoredDomains/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDomain(int id, [FromBody] MonitoredDomainCreateDto updateDto)
        {
            var existingDomain = await _context.MonitoredDomains.FindAsync(id);

            if (existingDomain == null)
            {
                return NotFound(new { message = $"Güncellenmek istenen {id} ID'li şirket bulunamadı." });
            }

            existingDomain.DomainName = updateDto.DomainName.ToLower().Trim();
            existingDomain.CompanyName = updateDto.CompanyName;
            existingDomain.ContactEmail = updateDto.ContactEmail;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 5. DELETE -> DELETE: api/v1/MonitoredDomains/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDomain(int id)
        {
            var domain = await _context.MonitoredDomains.FindAsync(id);

            if (domain == null)
            {
                return NotFound(new { message = $"Silinmek istenen {id} ID'li şirket bulunamadı." });
            }

            _context.MonitoredDomains.Remove(domain);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Şirket domaini takip listesinden başarıyla silindi." });
        }

        // =========================================================================
        // SİBER İSTİHBARAT VE ANALİZ MOTORU METOTLARI (YENİ EKLENEN KISIM)
        // =========================================================================

        // 6. SIZINTI SİMÜLASYONU -> POST: api/v1/MonitoredDomains/simulate-leaks
        [HttpPost("simulate-leaks")]
        public async Task<IActionResult> SimulateLeaks()
        {
            // Mükerrer kayıt olmaması için eski simülasyon verilerini temizleyelim
            var existingLeaks = await _context.LeakedCredentials.ToListAsync();
            if (existingLeaks.Any())
            {
                _context.LeakedCredentials.RemoveRange(existingLeaks);
                await _context.SaveChangesAsync();
            }

            // Örnek sızıntı verileri (Güvenlik amacıyla SHA256 şifre hash'leri tutuluyor)
            var mockLeaks = new List<LeakedCredential>
            {
                new LeakedCredential { Email = "ahmet@beykent.edu.tr", Domain = "beykent.edu.tr", PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", BreachSourceId = 1 },
                new LeakedCredential { Email = "mehmet@beykent.edu.tr", Domain = "beykent.edu.tr", PasswordHash = "4fa72a1e35d2d0c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", BreachSourceId = 1 },
                new LeakedCredential { Email = "siber@beykent.edu.tr", Domain = "beykent.edu.tr", PasswordHash = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b", BreachSourceId = 1 },
                new LeakedCredential { Email = "ceo@sirket.com", Domain = "sirket.com", PasswordHash = "fbc5a0e35d2d0c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", BreachSourceId = 1 }
            };

            _context.LeakedCredentials.AddRange(mockLeaks);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Siber istihbarat simülasyon verileri (Leaked Credentials) başarıyla yüklendi!" });
        }

        // 7. SİBER RİSK ANALİZ MOTORU -> POST: api/v1/MonitoredDomains/analyze/beykent.edu.tr
        [HttpPost("analyze/{domainName}")]
        public async Task<IActionResult> AnalyzeDomainRisk(string domainName)
        {
            var cleanedDomain = domainName.ToLower().Trim();

            // Şirket takip listemizde ekli mi?
            var domain = await _context.MonitoredDomains.FirstOrDefaultAsync(d => d.DomainName == cleanedDomain);
            if (domain == null)
            {
                return NotFound(new { message = $"Takip listesinde '{cleanedDomain}' adında bir domain bulunamadı." });
            }

            // Domaine ait sızıntı sayısını veritabanından çekiyoruz (İndeksli alan performansı)
            var leakCount = await _context.LeakedCredentials.CountAsync(lc => lc.Domain == cleanedDomain);

            // Risk Skoru Hesaplama Algoritması: Her sızıntı başı 25 puan, tavan sınır 100
            int calculatedScore = leakCount * 25;
            if (calculatedScore > 100) calculatedScore = 100;

            // Hesaplanan dinamik skoru veritabanına yansıtıyoruz
            domain.RiskScore = calculatedScore;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"'{cleanedDomain}' için siber risk analizi tamamlandı.",
                toplamSiziantiSayisi = leakCount,
                hesaplananSiberRiskSkoru = calculatedScore,
                durum = calculatedScore >= 75 ? "KRİTİK TEHLİKE" : calculatedScore >= 50 ? "YÜKSEK RİSK" : calculatedScore > 0 ? "ORTA TEHLİKE" : "GÜVENLİ"
            });
        }
    }
}