namespace Domain.Interfaces.Services
{
    public interface IMigrationService
    {
        // Método para realizar a migração dos grupos de acesso
        Task GroupAccessMigration();

        // Método para realizar a migração das categorias
        Task CategoryMigration();

        // Método para realizar a migração dos planos
        Task PlansMigration();

        Task GatewayConfiguration();

        Task DigitalStickerMigration();

        Task CreateSystemBankAccount();

        Task RenamePropertieInCampaign();

        Task CreateBAASConfiguration();
    }
}
