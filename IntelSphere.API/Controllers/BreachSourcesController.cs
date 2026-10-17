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

        // Importing our database object using Dependency Injection
        public BreachSourcesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. READ (List All) -> GET: api/v1/BreachSources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreachSourceResponseDto>>> GetSources()
        {
            var sources = await _context.BreachSources.ToListAsync();

            // We convert the database model into a DTO for the outside world
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
                return NotFound(new { message = $"The leak source with ID {id} could not be found." });
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
            // We map the clean data from the DTO to the actual database object
            var newSource = new BreachSource
            {
                SourceName = createDto.SourceName,
                BreachDate = createDto.BreachDate,
                Severity = createDto.Severity
            };

            _context.BreachSources.Add(newSource);
            await _context.SaveChangesAsync(); // Physical record in the database

            // We return the Response DTO version of the saved object
            var response = new BreachSourceResponseDto
            {
                Id = newSource.Id,
                SourceName = newSource.SourceName,
                BreachDate = newSource.BreachDate,
                Severity = newSource.Severity,
                CreatedAt = newSource.CreatedAt
            };

            // We specify the address where the data has been added using the 201 Created code (RESTful standard)
            return CreatedAtAction(nameof(GetSource), new { id = response.Id }, response);
        }

        // 4. UPDATE -> PUT: api/v1/BreachSources/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] BreachSourceCreateDto updateDto)
        {
            var existingSource = await _context.BreachSources.FindAsync(id);

            if (existingSource == null)
            {
                return NotFound(new { message = $"The resource with ID {id} that you wish to update could not be found." });
            }

            // We are updating the existing records with the latest data from the DTO
            existingSource.SourceName = updateDto.SourceName;
            existingSource.BreachDate = updateDto.BreachDate;
            existingSource.Severity = updateDto.Severity;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 NoContent -> Successfully updated; no need to return any data.
        }

        // 5. DELETE -> DELETE: api/v1/BreachSources/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSource(int id)
        {
            var source = await _context.BreachSources.FindAsync(id);

            if (source == null)
            {
                return NotFound(new { message = $"The resource with ID {id} that was intended to be deleted could not be found." });
            }

            _context.BreachSources.Remove(source);
            await _context.SaveChangesAsync();

            return Ok(new { message = "The leak source and all leak records associated with this source have been successfully deleted." });
        }
    }
}