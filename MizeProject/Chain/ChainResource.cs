namespace MizeProject.Chain;

public class ChainResource<T>(IEnumerable<IStorage<T>> storages)
{

    private readonly List<IStorage<T>> _storages = storages.OrderBy(s => s.Level).ToList();
    public async Task<Result<T>> GetValue()
    {
        Result<T> value = await RetrieveUpdateValueFromStoragesAsync();
        if (!value.HasValue)
            Console.WriteLine("Could not get valid value from any storage in the chain.");
        return value;
    }

    private async Task<Result<T>> RetrieveUpdateValueFromStoragesAsync()
    {
        T? foundValue;

        foreach (var storage in _storages)
        {
            if (!storage.IsRead)
                continue;

            var value = await storage.ReadAsync();

            if (value.HasValue && !storage.IsExpired)
            {
                foundValue = value.Value;
                await UpdateLowerStorageLevelsAsync(storage, foundValue!);
                return new Result<T>(true, foundValue!);
            }
        }

        return new Result<T>();
    }

    private async Task UpdateLowerStorageLevelsAsync(IStorage<T> currentStorage, T value)
    {
        int currentIndex = _storages.IndexOf(currentStorage);

        var writeStorages = _storages
            .Take(currentIndex)
            .Where(s => s.IsWrite);


        foreach (var storage in writeStorages)
        {
            try
            {
                await storage.WriteAsync(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to {storage.Level}: {ex.Message}");
            }
        }
    }
}
