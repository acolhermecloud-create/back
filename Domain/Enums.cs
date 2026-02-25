namespace Domain
{
    internal class Enums
    {
    }

    public enum AuthProvider
    {
        Google,
        Facebook,
        Apple,
        None // Para usuários que não utilizam autenticação social
    }

    public enum UserType
    {
        Common,
        Admin,
        Ong
    }

    public enum UserStatus
    {
        Active,
        Inactive
    }

    public enum CampaignStatus
    {
        Active,
        Closed,
        Approved,
        Rejected,
        Completed,
        Inactive
    }

    public enum CampaignisForWho
    {
        Me,
        ForSomeoneInTheFamily,
        ForMyPet,
        ForAFriendWhoIsInNeed,
        ForACompanyOrInstitution
    }

    public enum TransationMethod
    {
        Card,
        Cash,
        Store, // Loja interna
    }

    public enum DonationType
    {
        Money, // Dinheiro
        SmallDonations // Corações (curtidas)
    }

    public enum DonationStatus
    {
        Created,
        Paid,
        Canceled
    }

    public enum DonationBalanceStatus
    {
        None,
        Released,
        WaitingForRelease,
        CanceledDueToFraud,
        Withdrawn
    }

    public enum ComplaintType
    {
        Duplicity, // Duplicidade. Imagem, título e/ou descrição são idênticos aos de outra campanha
        MisuseOfSomeoneElsesImages, // Uso indevido de imagens de outra pessoa
        Offensive, // Conteúdo ofensivo e/ou que expõe nudez e violência.
        CommercialTransactions, // Transações comerciais envolvendo bens e/ou serviços. (lavagem)
        Othercircumstances // Outros
    }

    public enum ComplaintStatus
    {
        Open,
        Closed
    }

    public enum PlanType
    {
        Facil,
        Orange,
        Diamond
    }

    public enum Gateway
    {
        Iugu,
        ReflowPay,
        Internal,
        Bloobank,
        Transfeera,
        ReflowPayV2
    }

    public enum TransactionType
    {
        Created,
        Paid,
        Failed,
        Canceled,
    }

    public enum CampaignLogType
    {
        ReceiveDonation,
        ChangePrice,
        ChangeDescription,
        ChangeStatus,
        ChangeImages
    }

    public enum LeverageStatus
    {
        UnderReview,
        Rejected,
        Approved
    }

    public enum BankAccountType
    {
        Organization,
        Customer
    }

    public enum BankAccountTransactionType
    {
        CashIn,
        CashOut
    }

    public enum BankTransactionType
    {
        CashIn,  // Entrada de dinheiro
        CashOut  // Saída de dinheiro
    }

    public enum BankTransactionStatus
    {
        Pending,  // Aguardando processamento
        WaitingForRelease, // Aguardando liberação
        Completed, // Finalizada com sucesso
        Failed,   // Falha na transação
        AwaitingApproval,
        Canceled,
        Refund
    }

    public enum BankTransactionSource
    {
        Campaign,
        DigitalSticker
    }

    public enum BankSplitType
    {
        Percent,
        Value
    }
}
