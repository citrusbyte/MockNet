using SystemNameValueHeaderValue = System.Net.Http.Headers.NameValueHeaderValue;

namespace Theorem.MockNet.Http
{
    public class NameValueHeaderValue : IHeaderValue<SystemNameValueHeaderValue>
    {
        public static NameValueHeaderValue Parse(string input) => SystemNameValueHeaderValue.Parse(input);

        private readonly SystemNameValueHeaderValue value;

        internal NameValueHeaderValue(SystemNameValueHeaderValue value)
        {
            this.value = value;
        }

        public SystemNameValueHeaderValue GetValue() => value;

        public override string ToString() => value.ToString();

        public static implicit operator string(NameValueHeaderValue header) => header.ToString();
        public static implicit operator NameValueHeaderValue(string input) => new NameValueHeaderValue(Parse(input));
        public static implicit operator SystemNameValueHeaderValue(NameValueHeaderValue header) => header.GetValue();
        public static implicit operator NameValueHeaderValue(SystemNameValueHeaderValue header) => new NameValueHeaderValue(header);
    }
}