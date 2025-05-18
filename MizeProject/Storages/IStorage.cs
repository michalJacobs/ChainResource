namespace MizeProject.Storages
{
    public interface IStorage<T>
    {
        Task<Result<T>> ReadAsync();
        Task WriteAsync(T value);
        bool IsRead { get;  }
        bool IsWrite { get; }
        bool IsExpired { get; }
        T? StorageValue { get; }
        StorageLevel Level { get; }
    }
}
