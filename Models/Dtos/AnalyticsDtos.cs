using System.Collections.Generic;

namespace ArabamTR.Models.Dtos
{
    public class PriceAnalysisResponseDto
    {
        public int VehicleId { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal AverageMarketPrice { get; set; }
        public decimal SuggestedPrice { get; set; }
        public string PriceStatus { get; set; } = string.Empty; // Fiyatı Çok İyi, Normal, Yüksek
        public int ComparisonCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PriceTrendPointDto
    {
        public string Label { get; set; } = string.Empty; // e.g. "Ocak", "Şubat"
        public decimal AveragePrice { get; set; }
    }

    public class PriceTrendResponseDto
    {
        public string PeriodName { get; set; } = string.Empty; // "3-Aylık", "6-Aylık", "12-Aylık"
        public List<PriceTrendPointDto> Trends { get; set; } = new List<PriceTrendPointDto>();
    }
}
