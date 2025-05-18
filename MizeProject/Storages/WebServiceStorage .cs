namespace MizeProject.Storages
{
    public class WebServiceStorage<T>
        (StorageDefaults settings, HttpClient httpClient, Func<string, T> deserializeFunction) : IStorage<T>
    {
        private readonly string _apiUrl = settings.ExchangeRatesApiUrl;
        private readonly string _apiKey = settings.ExchangeRatesAppId;
        private readonly HttpClient _httpClient = httpClient;
        private Func<string, T> _deserializeFunction = deserializeFunction;

        public bool IsRead => true;
        public bool IsWrite => false;
        public bool IsExpired => false;
        public T? StorageValue { get; protected set; }
        public StorageLevel Level => StorageLevel.Web;

        public async Task<Result<T>> ReadAsync()
        {
            string url = $"{_apiUrl}?app_id={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var CachedValue = _deserializeFunction(json);

            if (CachedValue == null)
            {
                Console.WriteLine("Failed to deserialize exchange rates from web service.");
                return new Result<T>();
            }
            return new Result<T>(true, CachedValue);
        }

        public Task WriteAsync(T value)
        {
            throw new InvalidOperationException("WebServiceStorage is read-only.");
        }
    }
}
