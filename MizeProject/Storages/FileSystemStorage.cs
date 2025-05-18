using System.Text.Json;

namespace MizeProject.Storages
{
   
    public class FileSystemStorage<T>(StorageDefaults settings) :
        StorageBase<T>(settings.FileExpiration, true, true, StorageLevel.File)
    {
        private readonly string _filePath = 
            Path.Combine(Environment.GetFolderPath
                (Environment.SpecialFolder.LocalApplicationData), settings.ExchangeRatesFilePath);

        public override async Task<Result<T>> ReadAsync()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"File not found at path: {_filePath}");
                return new Result<T>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(_filePath);
                var fileData = JsonSerializer.Deserialize<FileStorageData<T>>(json);

                if (fileData == null || fileData.Value == null)
                {
                    Console.WriteLine("Invalid data in file storage.");
                    return new Result<T>();
                }
                LastUpdated = fileData.LastUpdated;
                StorageValue = fileData.Value;

                if (IsExpired)
                {
                    Console.WriteLine("Data in file storage is expired.");
                    return new Result<T>();
                }

                return new Result<T>(true, StorageValue);
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                Console.WriteLine($"Error reading or parsing file: {_filePath}");
                return new Result<T>();
            }
        }

        public override async Task WriteAsync(T value)
        {
            StorageValue = value;
            LastUpdated = DateTime.UtcNow;

            var fileData = new FileStorageData<T>
            {
                Value = value,
                LastUpdated = LastUpdated.Value
            };

            string json = JsonSerializer.Serialize(fileData);

            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_filePath, json);
        }

        private class FileStorageData<TData>
        {
            public TData? Value { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}
