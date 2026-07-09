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
    public class ModelController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;

        public ModelController(ArabamTRDbContext context)
        {
            _context = context;
        }

        // GET: api/model
        [HttpGet]
        public async Task<IActionResult> GetModels()
        {
            var models = await _context.Models.Include(m => m.Brand).ToListAsync();
            return Ok(models);
        }

        // GET: api/model/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetModel(int id)
        {
            var model = await _context.Models.Include(m => m.Brand).FirstOrDefaultAsync(m => m.Id == id);
            if (model == null)
                return NotFound("Model bulunamadı.");

            return Ok(model);
        }

        // POST: api/model
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateModel([FromBody] ModelCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var brandExists = await _context.Brands.AnyAsync(b => b.Id == dto.BrandId);
            if (!brandExists)
                return BadRequest("Geçersiz Marka ID.");

            var model = new Model
            {
                Name = dto.Name,
                BrandId = dto.BrandId
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
        }

        // PUT: api/model/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(int id, [FromBody] ModelUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = await _context.Models.FindAsync(id);
            if (model == null)
                return NotFound("Model bulunamadı.");

            var brandExists = await _context.Brands.AnyAsync(b => b.Id == dto.BrandId);
            if (!brandExists)
                return BadRequest("Geçersiz Marka ID.");

            model.Name = dto.Name;
            model.BrandId = dto.BrandId;
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // DELETE: api/model/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model == null)
                return NotFound("Model bulunamadı.");

            _context.Models.Remove(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Model başarıyla silindi." });
        }
    }
}
