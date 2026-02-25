namespace API.Payloads
{
    public class DashboardPayload
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
}
