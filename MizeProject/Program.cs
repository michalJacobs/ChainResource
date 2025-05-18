using Microsoft.Extensions.Configuration;
using MizeProject;
using MizeProject.Chain;
using MizeProject.Models;
using MizeProject.Storages;
using System.Text.Json;

public class Program
{
    public static async Task Main()
    {

        IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

        StorageDefaults storageDefaults = new();
        config.GetSection("StorageDefaults").Bind(storageDefaults);

        var memoryStorage = new MemoryStorage<ExchangeRateList>(storageDefaults);
        var fileStorage = new FileSystemStorage<ExchangeRateList>(storageDefaults);
        var webStorage = new WebServiceStorage<ExchangeRateList>(
            storageDefaults,
            new HttpClient(),
             json =>
            {
                return JsonSerializer.Deserialize<ExchangeRateList>(json, new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
            });

        var storageChain = new List<IStorage<ExchangeRateList>>
           {fileStorage,
            webStorage,
            memoryStorage };

        var exchangeRateResource = new ChainResource<ExchangeRateList>(storageChain);

        try
        {

            var ratesResult = await exchangeRateResource.GetValue();
            if (!ratesResult.HasValue)
            {
                Console.WriteLine("Failed to retrieve exchange rates.");
                return;
            }

            Console.WriteLine($"Base currency: {ratesResult.Value!.Base}");
            Console.WriteLine($"Timestamp: {DateTimeOffset.FromUnixTimeSeconds(ratesResult.Value.Timestamp).DateTime}");
            Console.WriteLine("Exchange rates:");

            foreach (var rate in ratesResult.Value.Rates)
            {
                Console.WriteLine($"{rate.Key}: {rate.Value}");
            }
            await exchangeRateResource.GetValue();
            await exchangeRateResource.GetValue();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}