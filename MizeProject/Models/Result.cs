namespace MizeProject.Models
{
    public class Result<T>
    {
        public bool HasValue { get; set; }
        public T? Value { get; set; }

        public Result(bool hasValue, T value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public Result(bool hasValue = false)
        {
            HasValue = hasValue;
        }
    }
}
