using Buckl.Application.Common;
using Buckl.Domain.Products;

namespace Buckl.Application.Products;

public sealed class ProductNotFoundException : ResourceNotFoundException
{
    public const string ErrorCode = "product.not_found";

    public ProductNotFoundException(ProductId productId)
        : base(ErrorCode, $"Product {productId} was not found.")
    {
        ProductId = productId;
    }

    public ProductId ProductId { get; }
}
