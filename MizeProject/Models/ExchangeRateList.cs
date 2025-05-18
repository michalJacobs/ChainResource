using CurrencyCode = System.String;
using ExchangeRate = System.Decimal;

namespace MizeProject.Models
{
    public class ExchangeRateList
    {
        public string? Base { get; set; }
        public Dictionary<CurrencyCode, ExchangeRate> Rates { get; set; } = [];
        public long Timestamp { get; set; }
    }
}