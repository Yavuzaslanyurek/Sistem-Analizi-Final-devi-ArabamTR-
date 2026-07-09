using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArabamTR.Data;
using ArabamTR.Models;
using ArabamTR.Models.Dtos;

namespace ArabamTR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;

        public AnalyticsController(ArabamTRDbContext context)
        {
            _context = context;
        }

        // GET: api/vehicle/query/{plateNumber} (Mapped using custom route as requested)
        [HttpGet("/api/vehicle/query/{plateNumber}")]
        public async Task<IActionResult> QueryVehicleHistory(string plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return BadRequest("Plaka numarası boş olamaz.");

            var normalizedPlate = plateNumber.Replace(" ", "").ToUpper();

            var history = await _context.FakeHistories.FirstOrDefaultAsync(h => h.PlateNumber == normalizedPlate);
            if (history != null)
            {
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
                    KmHistory = kmHistory,
                    Message = history.HasDamageRecord ? "Hasar kaydı bulunmaktadır." : "Temiz, hasar kaydı bulunamadı."
                });
            }

            // Fallback mock clean history instead of error
            var random = new Random();
            var mockKM = random.Next(35000, 115000);
            var mockKMHistory = new[]
            {
                new { Date = DateTime.UtcNow.AddYears(-2).ToString("yyyy-MM-dd"), KM = mockKM - 40000 },
                new { Date = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"), KM = mockKM - 20000 },
                new { Date = DateTime.UtcNow.ToString("yyyy-MM-dd"), KM = mockKM }
            };

            return Ok(new
            {
                PlateNumber = normalizedPlate,
                HasDamageRecord = false,
                DamageAmount = 0.00m,
                LastKM = mockKM,
                KmHistory = mockKMHistory,
                Message = "Temiz, hasar kaydı bulunamadı."
            });
        }

        // GET: api/analytics/price-analysis/{vehicleId}
        [HttpGet("price-analysis/{vehicleId}")]
        public async Task<IActionResult> GetPriceAnalysis(int vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
                return NotFound("Fiyat analizi yapılacak ilan bulunamadı.");

            // 1. Find comparison listings: Same model & same year
            var comparisons = await _context.Vehicles
                .Where(v => v.ModelId == vehicle.ModelId && v.Year == vehicle.Year && v.Id != vehicle.Id && v.Status == "Active")
                .ToListAsync();

            string scopeMessage = "Model ve Yıl bazlı karşılaştırma yapıldı.";

            // 2. Fallback: If no matches for same year, look up same model overall
            if (!comparisons.Any())
            {
                comparisons = await _context.Vehicles
                    .Where(v => v.ModelId == vehicle.ModelId && v.Id != vehicle.Id && v.Status == "Active")
                    .ToListAsync();
                scopeMessage = "Aynı yıla ait karşılaştırma aracı bulunamadığı için sadece Model bazlı karşılaştırma yapıldı.";
            }

            decimal averagePrice;
            decimal suggestedPrice;
            int comparisonCount = comparisons.Count;

            if (comparisons.Any())
            {
                averagePrice = comparisons.Average(c => c.Price);
                var averageKM = comparisons.Average(c => c.KM);

                // Odometer correction: -1.5% adjustment for every 15,000 KM above average, +1.5% for below.
                var kmDifference = vehicle.KM - averageKM;
                var adjustmentPercentage = -(kmDifference / 15000.0) * 0.015;
                
                // Clamp adjustment between -20% and +20%
                if (adjustmentPercentage < -0.20) adjustmentPercentage = -0.20;
                if (adjustmentPercentage > 0.20) adjustmentPercentage = 0.20;

                suggestedPrice = averagePrice * (decimal)(1.0 + adjustmentPercentage);
            }
            else
            {
                // Fallback: No comparisons found. Use current price as baseline.
                averagePrice = vehicle.Price;
                suggestedPrice = vehicle.Price;
                scopeMessage = "Sistemde karşılaştırma yapılacak başka araç bulunmadığı için ilan fiyatı piyasa ortalaması olarak kabul edilmiştir.";
            }

            // Determine smart price status (Akıllı Fiyat Önerisi)
            // Very Good: Current price is at least 5% below suggested price
            // High: Current price is at least 5% above suggested price
            // Normal: Within +/- 5% of suggested price
            string priceStatus;
            if (vehicle.Price <= suggestedPrice * 0.95m)
            {
                priceStatus = "Fiyatı Çok İyi";
            }
            else if (vehicle.Price >= suggestedPrice * 1.05m)
            {
                priceStatus = "Yüksek";
            }
            else
            {
                priceStatus = "Normal";
            }

            var result = new PriceAnalysisResponseDto
            {
                VehicleId = vehicle.Id,
                CurrentPrice = vehicle.Price,
                AverageMarketPrice = Math.Round(averagePrice, 2),
                SuggestedPrice = Math.Round(suggestedPrice, 2),
                PriceStatus = priceStatus,
                ComparisonCount = comparisonCount,
                Message = scopeMessage
            };

            return Ok(result);
        }

        // GET: api/analytics/price-trend/{modelId}
        [HttpGet("price-trend/{modelId}")]
        public async Task<IActionResult> GetPriceTrend(int modelId)
        {
            var modelExists = await _context.Models.AnyAsync(m => m.Id == modelId);
            if (!modelExists)
                return NotFound("Model bulunamadı.");

            // Determine baseline price for calculation (pull average price of this model or fallback to standard values)
            var basePrice = await _context.Vehicles
                .Where(v => v.ModelId == modelId)
                .Select(v => (decimal?)v.Price)
                .AverageAsync() ?? 950000.00m;

            var monthNames = new[] { "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran" };
            
            // Build trend points with minor price variations simulating market adjustments
            // 3 Months trend
            var trend3Months = new List<PriceTrendPointDto>();
            for (int i = 9; i < 12; i++)
            {
                var factor = 0.97m + (i - 9) * 0.015m; // slight positive trend
                trend3Months.Add(new PriceTrendPointDto
                {
                    Label = monthNames[i],
                    AveragePrice = Math.Round(basePrice * factor, 2)
                });
            }

            // 6 Months trend
            var trend6Months = new List<PriceTrendPointDto>();
            for (int i = 6; i < 12; i++)
            {
                var factor = 0.95m + (i - 6) * 0.01m;
                trend6Months.Add(new PriceTrendPointDto
                {
                    Label = monthNames[i],
                    AveragePrice = Math.Round(basePrice * factor, 2)
                });
            }

            // 12 Months trend
            var trend12Months = new List<PriceTrendPointDto>();
            for (int i = 0; i < 12; i++)
            {
                var factor = 0.92m + i * 0.008m;
                trend12Months.Add(new PriceTrendPointDto
                {
                    Label = monthNames[i],
                    AveragePrice = Math.Round(basePrice * factor, 2)
                });
            }

            var result = new
            {
                ModelId = modelId,
                BasePrice = basePrice,
                Trend3Months = trend3Months,
                Trend6Months = trend6Months,
                Trend12Months = trend12Months
            };

            return Ok(result);
        }
    }
}
