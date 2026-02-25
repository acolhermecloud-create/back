namespace API.Payloads
{
    public class RecordPlanPayload
    {
        public string? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Benefits { get; set; }
        public decimal PercentToBeCharged { get; set; }
        public decimal FixedRate { get; set; }
        public bool NeedApproval { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }
    }
}
