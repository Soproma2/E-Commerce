namespace E_Commerce.Common.DTOs.Responses
{
    public class Paged<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public Paged(List<T> items, int totalCount, int currentPage, int take)
        {
            Items = items;
            PageSize = take;
            TotalCount = totalCount;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            HasNextPage = TotalPages > CurrentPage;
        }
    };
}
