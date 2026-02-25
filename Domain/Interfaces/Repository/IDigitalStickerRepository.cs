namespace Domain.Interfaces.Repository
{
    public interface IDigitalStickerRepository
    {
        Task<DigitalSticker> GetById(Guid id);

        Task Add(DigitalSticker digitalSticker);

        Task<List<DigitalSticker>> List();
    }
}
