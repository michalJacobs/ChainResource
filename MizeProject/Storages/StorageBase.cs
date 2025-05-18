namespace MizeProject.Storages
{
    public abstract class StorageBase<T>
        (TimeSpan expirationInterval, bool canRead, bool canWrite, StorageLevel Level)
        : IStorage<T>
    {
        public DateTime? LastUpdated { get; set; } 
        protected TimeSpan ExpirationInterval { get; } = expirationInterval;
        public StorageLevel Level { get; } = Level;
        public T? StorageValue { get; protected set; }
        public bool IsRead { get; protected set; } = canRead;
        public bool IsWrite { get; protected set; } = canWrite;

        public bool IsExpired
        {
            get
            {
                if (!LastUpdated.HasValue)
                    return true;

                return DateTime.UtcNow - LastUpdated.Value > ExpirationInterval;
            }
        }
        public abstract Task<Result<T>> ReadAsync();
        public abstract Task WriteAsync(T value);
    }
}
