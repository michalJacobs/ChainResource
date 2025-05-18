
namespace MizeProject.Storages
{
    public class MemoryStorage<T>(StorageDefaults settings) :
        StorageBase<T>(settings.MemoryExpiration, true, true ,StorageLevel.Memory)
    {
        public override Task<Result<T>> ReadAsync()
        {
            if (StorageValue == null)
            {
                Console.WriteLine("No data available in memory storage.");
                return Task.FromResult(new Result<T>());
            }

            return Task.FromResult(new Result<T>(true,StorageValue));
        }
        
        public override Task WriteAsync(T value)
        {
            StorageValue = value;
            LastUpdated = DateTime.UtcNow;
            return Task.CompletedTask;
        }
    }
}


