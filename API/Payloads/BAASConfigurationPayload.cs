namespace API.Payloads
{
    public class BAASConfigurationPayload
    {
        public bool AnalyseWithdraw { get; set; }
        public long DailyWithdrawalLimitValue { get; set; }
        public long DailyWithdrawalMinimumValue { get; set; }
    }
}
