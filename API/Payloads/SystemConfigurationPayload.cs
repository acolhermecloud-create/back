using Domain;

namespace API.Payloads
{
    public class SystemConfigurationPayload
    {
        public Gateway Pix { get; set; }
        public Gateway Card { get; set; }
        public bool TryToGenerateCashInInOtherAcquirers { get; set; }
    }
}
