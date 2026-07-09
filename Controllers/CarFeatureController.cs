using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ArabamTR.Data;
using ArabamTR.Models;
using ArabamTR.Models.Dtos;

namespace ArabamTR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarFeatureController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;

        public CarFeatureController(ArabamTRDbContext context)
        {
            _context = context;
        }

        // GET: api/carfeature
        [HttpGet]
        public async Task<IActionResult> GetFeatures()
        {
            var features = await _context.CarFeatures.ToListAsync();
            return Ok(features);
        }

        // GET: api/carfeature/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeature(int id)
        {
            var feature = await _context.CarFeatures.FindAsync(id);
            if (feature == null)
                return NotFound("Özellik bulunamadı.");

            return Ok(feature);
        }

        // POST: api/carfeature
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateFeature([FromBody] CarFeatureCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var feature = new CarFeature
            {
                FeatureName = dto.FeatureName,
                FeatureType = dto.FeatureType
            };

            _context.CarFeatures.Add(feature);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFeature), new { id = feature.Id }, feature);
        }

        // PUT: api/carfeature/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeature(int id, [FromBody] CarFeatureUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var feature = await _context.CarFeatures.FindAsync(id);
            if (feature == null)
                return NotFound("Özellik bulunamadı.");

            feature.FeatureName = dto.FeatureName;
            feature.FeatureType = dto.FeatureType;
            await _context.SaveChangesAsync();

            return Ok(feature);
        }

        // DELETE: api/carfeature/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeature(int id)
        {
            var feature = await _context.CarFeatures.FindAsync(id);
            if (feature == null)
                return NotFound("Özellik bulunamadı.");

            // Deleting the CarFeature will cascade delete all linked records in VehicleFeature table 
            // since we configured OnDelete(DeleteBehavior.Cascade) in ArabamTRDbContext.
            _context.CarFeatures.Remove(feature);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Özellik sistemden ve ilişkili tüm ilanlardan başarıyla silindi." });
        }
    }
}
