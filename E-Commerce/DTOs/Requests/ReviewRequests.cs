using E_Commerce.Common.DTOs.Requests;

namespace E_Commerce.DTOs.Requests;

public class CreateReviewRequest
{
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class FilterReviewsRequest : PagedRequest
{
    public int ProductId { get; set; }
}
