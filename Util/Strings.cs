namespace Util
{
    public static class Strings
    {
        public static Dictionary<string, string> Webhooks = new()
        {
            { "donation", "/campaign/donation/pix/confirm" },
            { "digitalStickers", "/store/payment/digitalStickers/confirm" },
        };
    }
}
