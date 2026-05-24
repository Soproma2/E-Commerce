using E_Commerce.Common.DTOs.Responses;
using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Products;

public interface IProductServices
{
    Task<Result<Paged<ProductResponse>>> GetProducts(FilterProductsRequest request);
    Task<Result<ProductDetailsResponse>> GetProductById(int id);
    Task<Result<ProductDetailsResponse>> CreateProduct(CreateProductRequest request);
    Task<Result<ProductDetailsResponse>> UpdateProduct(int id, UpdateProductRequest request);
    Task<Result<bool>> DeleteProduct(int id);
}
