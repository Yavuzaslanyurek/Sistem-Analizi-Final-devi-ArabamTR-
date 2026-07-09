using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using ArabamTR.Data;

namespace ArabamTR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlateInquiryController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;

        public PlateInquiryController(ArabamTRDbContext context)
        {
            _context = context;
        }

        // GET: api/plateinquiry/34ABC123
        [HttpGet("{plateNumber}")]
        public async Task<IActionResult> InquiryPlate(string plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return BadRequest("Plaka numarası boş olamaz.");

            // Normalize plate number (remove spaces, uppercase)
            var normalizedPlate = plateNumber.Replace(" ", "").ToUpper();

            var history = await _context.FakeHistories.FirstOrDefaultAsync(h => h.PlateNumber == normalizedPlate);
            if (history == null)
                return NotFound(new { message = "Bu plakaya ait bir hasar/sorgu kaydı bulunamadı." });

            // Deserialize the KmHistoryJson into a readable object/structure
            object? kmHistory = null;
            try
            {
                if (!string.IsNullOrEmpty(history.KmHistoryJson))
                {
                    kmHistory = JsonSerializer.Deserialize<object>(history.KmHistoryJson);
                }
            }
            catch
            {
                kmHistory = history.KmHistoryJson;
            }

            return Ok(new
            {
                PlateNumber = history.PlateNumber,
                HasDamageRecord = history.HasDamageRecord,
                DamageAmount = history.DamageAmount,
                LastKM = history.LastKM,
                KmHistory = kmHistory
            });
        }
    }
}
