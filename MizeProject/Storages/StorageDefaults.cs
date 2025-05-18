namespace MizeProject.Storages
{
    public class StorageDefaults
    {
        public TimeSpan MemoryExpiration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan FileExpiration { get; set; } = TimeSpan.FromHours(4);
        public string ExchangeRatesFilePath { get; set; } = "exchange-rates.json";
        public string ExchangeRatesApiUrl { get; set; } = "https://openexchangerates.org/api/latest.json";
        public string ExchangeRatesAppId { get; set; } = "";
    }

}

