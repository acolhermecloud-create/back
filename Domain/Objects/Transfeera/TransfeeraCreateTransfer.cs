namespace Domain.Objects.Transfeera
{
    public class TransfeeraCreateTransfer
    {
        public DestinationBankAccount DestinationBankAccount { get; set; }

        public PixKeyValidation PixKeyValidation { get; set; }

        public decimal Value { get; set; }
    }

    public class PixKeyValidation
    {
        public string CpfCnpj { get; set; }
    }
}
