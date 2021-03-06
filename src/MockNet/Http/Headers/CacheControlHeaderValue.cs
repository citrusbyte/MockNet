using SystemCacheControlHeaderValue = System.Net.Http.Headers.CacheControlHeaderValue;

namespace Theorem.MockNet.Http
{
    public class CacheControlHeaderValue : IHeaderValue<SystemCacheControlHeaderValue>
    {
        public static CacheControlHeaderValue Parse(string input) => SystemCacheControlHeaderValue.Parse(input);

        private readonly SystemCacheControlHeaderValue value;

        internal CacheControlHeaderValue(SystemCacheControlHeaderValue value)
        {
            this.value = value;
        }

        public SystemCacheControlHeaderValue GetValue() => value;

        public override string ToString() => value.ToString();

        public static implicit operator string(CacheControlHeaderValue header) => header.ToString();
        public static implicit operator CacheControlHeaderValue(string input) => new CacheControlHeaderValue(Parse(input));
        public static implicit operator SystemCacheControlHeaderValue(CacheControlHeaderValue header) => header.GetValue();
        public static implicit operator CacheControlHeaderValue(SystemCacheControlHeaderValue header) => new CacheControlHeaderValue(header);
    }
}