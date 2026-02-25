using Domain;

namespace API.Payloads
{
    public class LeverageRequestUpdateStatusPayload
    {
        public Guid Id { get; set; }
        public LeverageStatus Status { get; set; }
    }
}
