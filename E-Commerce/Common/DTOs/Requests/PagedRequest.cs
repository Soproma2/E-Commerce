namespace E_Commerce.Common.DTOs.Requests
{
    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int Take { get; set; } = 10;
    }
}
