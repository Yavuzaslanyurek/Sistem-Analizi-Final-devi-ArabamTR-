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
    public class BrandController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;

        public BrandController(ArabamTRDbContext context)
        {
            _context = context;
        }

        // GET: api/brand
        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _context.Brands.Include(b => b.Models).ToListAsync();
            return Ok(brands);
        }

        // GET: api/brand/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBrand(int id)
        {
            var brand = await _context.Brands.Include(b => b.Models).FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                return NotFound("Marka bulunamadı.");

            return Ok(brand);
        }

        // POST: api/brand
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] BrandCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var brand = new Brand
            {
                Name = dto.Name
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, brand);
        }

        // PUT: api/brand/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] BrandUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return NotFound("Marka bulunamadı.");

            brand.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(brand);
        }

        // DELETE: api/brand/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.Include(b => b.Models).FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                return NotFound("Marka bulunamadı.");

            // Check if there are models under this brand, to avoid leaving orphan models or causing SQL constraint issues.
            // (or we can let cascade delete handle it, but models also have vehicles, so checking makes it clean).
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Marka başarıyla silindi." });
        }
    }
}
