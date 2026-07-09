using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArabamTR.Data;
using ArabamTR.Models;
using ArabamTR.Models.Dtos;

namespace ArabamTR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;

        public VehicleController(ArabamTRDbContext context)
        {
            _context = context;
        }

        // GET: api/vehicle
        [HttpGet]
        public async Task<IActionResult> GetVehicles(
            [FromQuery] int? brandId,
            [FromQuery] int? modelId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear,
            [FromQuery] string? search,
            [FromQuery] List<int>? featureIds)
        {
            var query = _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Include(v => v.VehicleFeatures)
                    .ThenInclude(vf => vf.Feature)
                .AsQueryable();

            // Filters
            if (brandId.HasValue)
            {
                query = query.Where(v => v.Model.BrandId == brandId.Value);
            }

            if (modelId.HasValue)
            {
                query = query.Where(v => v.ModelId == modelId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(v => v.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(v => v.Price <= maxPrice.Value);
            }

            if (minYear.HasValue)
            {
                query = query.Where(v => v.Year >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                query = query.Where(v => v.Year <= maxYear.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(v => v.Title.ToLower().Contains(lowerSearch) || 
                                         v.Description.ToLower().Contains(lowerSearch) ||
                                         v.PlateNumber.ToLower().Contains(lowerSearch));
            }

            // Dynamic AND matching for multiple feature IDs
            if (featureIds != null && featureIds.Any())
            {
                foreach (var fid in featureIds)
                {
                    var tempFid = fid;
                    query = query.Where(v => v.VehicleFeatures.Any(vf => vf.FeatureId == tempFid));
                }
            }

            var vehicles = await query.ToListAsync();

            var response = vehicles.Select(v => MapToResponseDto(v)).ToList();

            return Ok(response);
        }

        // GET: api/vehicle/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Include(v => v.VehicleFeatures)
                    .ThenInclude(vf => vf.Feature)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound("İlan bulunamadı.");

            return Ok(MapToResponseDto(vehicle));
        }

        // POST: api/vehicle
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] VehicleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Fetch current user ID from token claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized("Geçersiz kullanıcı oturumu.");

            // Verify model exists
            var modelExists = await _context.Models.AnyAsync(m => m.Id == dto.ModelId);
            if (!modelExists)
                return BadRequest("Seçilen model geçerli değil.");

            var vehicle = new Vehicle
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                KM = dto.KM,
                Year = dto.Year,
                PlateNumber = dto.PlateNumber,
                ChassisNumber = dto.ChassisNumber,
                EngineNumber = dto.EngineNumber,
                CreatedDate = DateTime.UtcNow,
                Status = "Active",
                UserId = userId,
                ModelId = dto.ModelId
            };

            // Add selected features if any
            if (dto.FeatureIds != null && dto.FeatureIds.Any())
            {
                var existingFeatureIds = await _context.CarFeatures
                    .Where(cf => dto.FeatureIds.Contains(cf.Id))
                    .Select(cf => cf.Id)
                    .ToListAsync();

                foreach (var featureId in existingFeatureIds)
                {
                    vehicle.VehicleFeatures.Add(new VehicleFeature
                    {
                        FeatureId = featureId
                    });
                }
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Load relations to return full DTO
            var createdVehicle = await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Include(v => v.VehicleFeatures)
                    .ThenInclude(vf => vf.Feature)
                .FirstAsync(v => v.Id == vehicle.Id);

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, MapToResponseDto(createdVehicle));
        }

        // PUT: api/vehicle/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleFeatures)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound("Güncellenmek istenen ilan bulunamadı.");

            // Verify current user ownership
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || vehicle.UserId != userId)
                return Forbid("Bu ilanı güncelleme yetkiniz bulunmamaktadır.");

            // Verify model exists
            var modelExists = await _context.Models.AnyAsync(m => m.Id == dto.ModelId);
            if (!modelExists)
                return BadRequest("Seçilen model geçerli değil.");

            vehicle.Title = dto.Title;
            vehicle.Description = dto.Description;
            vehicle.Price = dto.Price;
            vehicle.KM = dto.KM;
            vehicle.Year = dto.Year;
            vehicle.PlateNumber = dto.PlateNumber;
            vehicle.ChassisNumber = dto.ChassisNumber;
            vehicle.EngineNumber = dto.EngineNumber;
            vehicle.Status = dto.Status;
            vehicle.ModelId = dto.ModelId;

            // Remove existing features
            _context.VehicleFeatures.RemoveRange(vehicle.VehicleFeatures);

            // Add new features
            if (dto.FeatureIds != null && dto.FeatureIds.Any())
            {
                var existingFeatureIds = await _context.CarFeatures
                    .Where(cf => dto.FeatureIds.Contains(cf.Id))
                    .Select(cf => cf.Id)
                    .ToListAsync();

                foreach (var featureId in existingFeatureIds)
                {
                    vehicle.VehicleFeatures.Add(new VehicleFeature
                    {
                        FeatureId = featureId
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Load updated relations
            var updatedVehicle = await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Include(v => v.VehicleFeatures)
                    .ThenInclude(vf => vf.Feature)
                .FirstAsync(v => v.Id == vehicle.Id);

            return Ok(MapToResponseDto(updatedVehicle));
        }

        // DELETE: api/vehicle/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound("Silinmek istenen ilan bulunamadı.");

            // Verify ownership
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || vehicle.UserId != userId)
                return Forbid("Bu ilanı silme yetkiniz bulunmamaktadır.");

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan başarıyla silindi." });
        }

        // PUT: api/vehicle/5/features
        [Authorize]
        [HttpPut("{id}/features")]
        public async Task<IActionResult> UpdateVehicleFeatures(int id, [FromBody] List<int> featureIds)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleFeatures)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound("İlan bulunamadı.");

            // Verify current user ownership
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || vehicle.UserId != userId)
                return Forbid("Bu ilanın özelliklerini güncelleme yetkiniz bulunmamaktadır.");

            // Remove existing features
            _context.VehicleFeatures.RemoveRange(vehicle.VehicleFeatures);

            // Add new features
            if (featureIds != null && featureIds.Any())
            {
                var existingFeatureIds = await _context.CarFeatures
                    .Where(cf => featureIds.Contains(cf.Id))
                    .Select(cf => cf.Id)
                    .ToListAsync();

                foreach (var featureId in existingFeatureIds)
                {
                    vehicle.VehicleFeatures.Add(new VehicleFeature
                    {
                        FeatureId = featureId
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Load updated relations
            var updatedVehicle = await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Include(v => v.VehicleFeatures)
                    .ThenInclude(vf => vf.Feature)
                .FirstAsync(v => v.Id == vehicle.Id);

            return Ok(MapToResponseDto(updatedVehicle));
        }

        private static VehicleResponseDto MapToResponseDto(Vehicle v)
        {
            return new VehicleResponseDto
            {
                Id = v.Id,
                Title = v.Title,
                Description = v.Description,
                Price = v.Price,
                KM = v.KM,
                Year = v.Year,
                PlateNumber = v.PlateNumber,
                ChassisNumber = v.ChassisNumber,
                EngineNumber = v.EngineNumber,
                CreatedDate = v.CreatedDate,
                Status = v.Status,
                UserId = v.UserId,
                UserName = v.User?.Name ?? string.Empty,
                ModelId = v.ModelId,
                ModelName = v.Model?.Name ?? string.Empty,
                BrandId = v.Model?.BrandId ?? 0,
                BrandName = v.Model?.Brand?.Name ?? string.Empty,
                Features = v.VehicleFeatures.Select(vf => new CarFeatureDto
                {
                    Id = vf.FeatureId,
                    FeatureName = vf.Feature?.FeatureName ?? string.Empty,
                    FeatureType = vf.Feature?.FeatureType ?? string.Empty
                }).ToList()
            };
        }
    }
}
