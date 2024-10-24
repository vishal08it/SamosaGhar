namespace SamosaGhar.Config
{
    public class RazorpayConfig
    {
        public string Key { get; }
        public string Secret { get; }

        public RazorpayConfig(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }
    }
}
