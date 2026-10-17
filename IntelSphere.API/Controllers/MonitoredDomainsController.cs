using System;
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
                return NotFound(new { message = $"Monitored domain with ID {id} was not found." });
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
                // Domain should be standard lowercase and clean.
                // ToLowerInvariant avoids culture-specific results (for example the Turkish dotless i).
                DomainName = createDto.DomainName.ToLowerInvariant().Trim(),
                CompanyName = createDto.CompanyName,
                ContactEmail = createDto.ContactEmail,
                RiskScore = 0 // The risk score of the newly added company is initially 0.
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
                return NotFound(new { message = $"The company with ID {id} requested for update was not found." });
            }

            existingDomain.DomainName = updateDto.DomainName.ToLowerInvariant().Trim();
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
                return NotFound(new { message = $"The company with ID {id} requested for deletion was not found." });
            }

            _context.MonitoredDomains.Remove(domain);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Company domain successfully deleted from the tracking list." });
        }

        // =========================================================================
        // CYBER INTELLIGENCE AND ANALYSIS ENGINE METHODS
        // =========================================================================

        // 6. LEAK SIMULATION -> POST: api/v1/MonitoredDomains/simulate-leaks
        [HttpPost("simulate-leaks")]
        public async Task<IActionResult> SimulateLeaks()
        {
            // Let's clean up old simulation data to prevent duplicate records
            var existingLeaks = await _context.LeakedCredentials.ToListAsync();
            if (existingLeaks.Any())
            {
                _context.LeakedCredentials.RemoveRange(existingLeaks);
                await _context.SaveChangesAsync();
            }

            // Attach the sample records to an existing breach source, or create one if none exists
            var source = await _context.BreachSources.OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (source == null)
            {
                source = new BreachSource
                {
                    SourceName = "Sample Forum Dump",
                    BreachDate = new DateTime(2024, 1, 15),
                    CreatedAt = DateTime.UtcNow,
                    Severity = "High"
                };
                _context.BreachSources.Add(source);
                await _context.SaveChangesAsync();
            }

            // Synthetic sample data only: reserved example domains (RFC 2606) and placeholder hashes.
            var now = DateTime.UtcNow;
            var mockLeaks = new List<LeakedCredential>
            {
                new LeakedCredential { Email = "alice@example.edu", Domain = "example.edu", PasswordHash = new string('a', 64), BreachSourceId = source.Id, DiscoveredAt = now },
                new LeakedCredential { Email = "bob@example.edu", Domain = "example.edu", PasswordHash = new string('b', 64), BreachSourceId = source.Id, DiscoveredAt = now },
                new LeakedCredential { Email = "carol@example.edu", Domain = "example.edu", PasswordHash = new string('c', 64), BreachSourceId = source.Id, DiscoveredAt = now },
                new LeakedCredential { Email = "ceo@example.com", Domain = "example.com", PasswordHash = new string('d', 64), BreachSourceId = source.Id, DiscoveredAt = now }
            };

            _context.LeakedCredentials.AddRange(mockLeaks);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cyber intelligence simulation data (Leaked Credentials) successfully loaded!" });
        }

        // 7. CYBER RISK ANALYSIS ENGINE -> POST: api/v1/MonitoredDomains/analyze/example.edu
        [HttpPost("analyze/{domainName}")]
        public async Task<IActionResult> AnalyzeDomainRisk(string domainName)
        {
            var cleanedDomain = domainName.ToLowerInvariant().Trim();

            // Is the company added to our tracking list?
            var domain = await _context.MonitoredDomains.FirstOrDefaultAsync(d => d.DomainName == cleanedDomain);
            if (domain == null)
            {
                return NotFound(new { message = $"No domain named '{cleanedDomain}' was found in the tracking list." });
            }

            // Fetching the leak count belonging to the domain from the database (Indexed field performance)
            var leakCount = await _context.LeakedCredentials.CountAsync(lc => lc.Domain == cleanedDomain);

            // Risk Score Calculation Algorithm: 25 points per leak, ceiling limit 100
            int calculatedScore = leakCount * 25;
            if (calculatedScore > 100) calculatedScore = 100;

            // Reflecting the calculated dynamic score to the database
            domain.RiskScore = calculatedScore;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Cyber risk analysis completed for '{cleanedDomain}'.",
                totalLeakCount = leakCount,
                calculatedCyberRiskScore = calculatedScore,
                status = calculatedScore >= 75 ? "CRITICAL DANGER" : calculatedScore >= 50 ? "HIGH RISK" : calculatedScore > 0 ? "MEDIUM DANGER" : "SECURE"
            });
        }
    }
}
