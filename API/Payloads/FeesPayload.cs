namespace API.Payloads
{
    public class FeesPayload
    {
        public decimal Fixed { get; set; }
        public decimal Variable { get; set; }

        public decimal BaasFixed { get; set; } = 0;
        public decimal BaasVariable { get; set; } = 0;
    }
}
