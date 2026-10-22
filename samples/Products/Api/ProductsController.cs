using Microsoft.AspNetCore.Mvc;

namespace Products.Sample;

public sealed record CreateProductRequest(string Name, decimal Price);

[ApiController]
[Route("products")]
public sealed class ProductsController(ICreateProductUseCase create, IGetProductUseCase get) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProductOutput>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await create.ExecuteAsync(new CreateProductInput(request.Name, request.Price), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid product", Detail = error.Message, Status = 400 });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductOutput>> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await get.ExecuteAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
