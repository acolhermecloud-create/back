namespace Domain.Objects
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public long TotalItems { get; set; }
        public bool HasMoreItems { get; set; }
    }
}
