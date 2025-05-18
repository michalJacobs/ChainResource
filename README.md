# ChainResource - Cached Resource Chain

## Overview

ChainResource is a generic resource provider implementation with layered storage support. It implements a chain of responsibility pattern for resource access, allowing data to be stored and retrieved from multiple storage locations with different performance characteristics.

## Features

- **Layered Storage System**: Supports multiple storage types (memory, file system, web service)
- **Automatic Expiration**: Implements expiration logic for cached data
- **Propagation Logic**: When data is retrieved from a deeper storage level, it's automatically propagated to higher-level storages
- **Generic Implementation**: Can be used with any data type
- **Asynchronous Operations**: All data access is asynchronous using Task-based patterns

## Architecture

The system is organized in layers:

1. **Memory Storage** (fastest, 1-hour expiration)
2. **File System Storage** (medium speed, 4-hour expiration)
3. **Web Service Storage** (slowest, read-only)

When a resource is requested, the system checks each storage level in sequence. If a valid (non-expired) resource is found, it is returned and propagated upward to faster storage levels.

## Implementation Details

The project consists of several key components:

- **ChainResource<T>**: Main entry point that orchestrates the chain of storages
- **IStorage<T>**: Interface defining the storage contract
- **StorageBase<T>**: Abstract base class implementing common storage functionality
- **Concrete Storage Implementations**:
  - MemoryStorage<T>: In-memory caching with the fastest access
  - FileSystemStorage<T>: JSON file-based storage for persistence between application restarts
  - WebServiceStorage<T>: API-based storage for retrieving fresh data

## Example Usage

A practical example using ExchangeRateList:

```csharp
// Create storage instances
var memoryStorage = new MemoryStorage<ExchangeRateList>(storageDefaults);
var fileStorage = new FileSystemStorage<ExchangeRateList>(storageDefaults);
var webStorage = new WebServiceStorage<ExchangeRateList>(
    storageDefaults,
    new HttpClient(),
    json => JsonSerializer.Deserialize<ExchangeRateList>(json)!);

// Create the chain
var storages = new List<IStorage<ExchangeRateList>>
{
    memoryStorage,
    fileStorage,
    webStorage
};

var exchangeRateResource = new ChainResource<ExchangeRateList>(storages);

// Get the exchange rates
var result = await exchangeRateResource.GetValue();
if (result.HasValue)
{
    Console.WriteLine($"Base currency: {result.Value!.Base}");
    Console.WriteLine($"Timestamp: {DateTimeOffset.FromUnixTimeSeconds(result.Value.Timestamp).DateTime}");
    Console.WriteLine("Exchange rates:");

    foreach (var rate in result.Value.Rates)
    {
        Console.WriteLine($"{rate.Key}: {rate.Value}");
    }
}
```

## Configuration

The storage behavior can be configured using the `StorageDefaults` class:

```csharp
var settings = new StorageDefaults
{
    MemoryExpiration = TimeSpan.FromHours(1),
    FileExpiration = TimeSpan.FromHours(4),
    ExchangeRatesFilePath = "exchange-rates.json",
    ExchangeRatesApiUrl = "https://api.example.com/rates",
    ExchangeRatesAppId = "your-api-key"
};
```

## Project Structure

- **MizeProject**: Main project containing the core implementation
  - **Chain**: Contains the ChainResource implementation
  - **Models**: Contains data models like ExchangeRateList and Result
  - **Storages**: Contains storage implementations

## Requirements

- .NET 6.0 or higher
- System.Text.Json for JSON serialization/deserialization

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---
