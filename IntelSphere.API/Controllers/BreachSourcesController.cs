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
    public class BreachSourcesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Dependency Injection ile veritabanı nesnemizi içeri alıyoruz
        public BreachSourcesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. READ (List All) -> GET: api/v1/BreachSources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreachSourceResponseDto>>> GetSources()
        {
            var sources = await _context.BreachSources.ToListAsync();

            // Veritabanı modelini dış dünyaya DTO olarak dönüştürüyoruz
            var response = sources.Select(s => new BreachSourceResponseDto
            {
                Id = s.Id,
                SourceName = s.SourceName,
                BreachDate = s.BreachDate,
                Severity = s.Severity,
                CreatedAt = s.CreatedAt
            }).ToList();

            return Ok(response);
        }

        // 2. READ (Get By Id) -> GET: api/v1/BreachSources/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BreachSourceResponseDto>> GetSource(int id)
        {
            var source = await _context.BreachSources.FindAsync(id);

            if (source == null)
            {
                return NotFound(new { message = $"ID'si {id} olan sızıntı kaynağı bulunamadı." });
            }

            var response = new BreachSourceResponseDto
            {
                Id = source.Id,
                SourceName = source.SourceName,
                BreachDate = source.BreachDate,
                Severity = source.Severity,
                CreatedAt = source.CreatedAt
            };

            return Ok(response);
        }

        // 3. CREATE -> POST: api/v1/BreachSources
        [HttpPost]
        public async Task<ActionResult<BreachSourceResponseDto>> CreateSource([FromBody] BreachSourceCreateDto createDto)
        {
            // DTO'dan gelen temiz veriyi gerçek Veritabanı nesnesine eşliyoruz
            var newSource = new BreachSource
            {
                SourceName = createDto.SourceName,
                BreachDate = createDto.BreachDate,
                Severity = createDto.Severity
            };

            _context.BreachSources.Add(newSource);
            await _context.SaveChangesAsync(); // Veritabanına fiziksel kayıt

            // Geriye kaydedilen nesnenin Response DTO halini dönüyoruz
            var response = new BreachSourceResponseDto
            {
                Id = newSource.Id,
                SourceName = newSource.SourceName,
                BreachDate = newSource.BreachDate,
                Severity = newSource.Severity,
                CreatedAt = newSource.CreatedAt
            };

            // 201 Created kodu ile verinin eklendiği adresi belirtiyoruz (RESTful standart)
            return CreatedAtAction(nameof(GetSource), new { id = response.Id }, response);
        }

        // 4. UPDATE -> PUT: api/v1/BreachSources/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] BreachSourceCreateDto updateDto)
        {
            var existingSource = await _context.BreachSources.FindAsync(id);

            if (existingSource == null)
            {
                return NotFound(new { message = $"Güncellenmek istenen {id} ID'li kaynak bulunamadı." });
            }

            // Mevcut kayıtları DTO'dan gelen güncel verilerle yeniliyoruz
            existingSource.SourceName = updateDto.SourceName;
            existingSource.BreachDate = updateDto.BreachDate;
            existingSource.Severity = updateDto.Severity;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 NoContent -> Başarıyla güncellendi, geriye veri dönmeye gerek yok.
        }

        // 5. DELETE -> DELETE: api/v1/BreachSources/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSource(int id)
        {
            var source = await _context.BreachSources.FindAsync(id);

            if (source == null)
            {
                return NotFound(new { message = $"Silinmek istenen {id} ID'li kaynak bulunamadı." });
            }

            _context.BreachSources.Remove(source);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sızıntı kaynağı ve bu kaynağa bağlı tüm sızıntı kayıtları başarıyla silindi." });
        }
    }
}