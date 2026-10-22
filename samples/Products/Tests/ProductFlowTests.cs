using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Products.Sample;
using Xunit;

namespace Products.Tests;

public sealed class ProductFlowTests
{
    [Fact]
    public async Task Given_valid_product_when_created_then_http_location_reads_the_same_product()
    {
        await using var app = ProductApi.Create(Array.Empty<string>());
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            using var client = Client(app.Services);
            using var response = await client.PostAsJsonAsync("products", new { name = "  Notebook  ", price = 12.5m });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ProductOutput>();
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal("Notebook", created.Name);
            var read = await client.GetFromJsonAsync<ProductOutput>(response.Headers.Location);
            Assert.Equal(created, read);
        }
        finally { await app.StopAsync(); }
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("Product", -1)]
    public async Task Given_invalid_product_when_posted_then_http_returns_bad_request(string name, decimal price)
    {
        await using var app = ProductApi.Create(Array.Empty<string>());
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            using var client = Client(app.Services);
            using var response = await client.PostAsJsonAsync("products", new { name, price });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally { await app.StopAsync(); }
    }

    [Fact]
    public async Task Given_missing_product_when_read_then_http_returns_not_found()
    {
        await using var app = ProductApi.Create(Array.Empty<string>());
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            using var client = Client(app.Services);
            using var response = await client.GetAsync("products/" + Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally { await app.StopAsync(); }
    }

    [Fact]
    public async Task Given_invalid_input_when_use_case_executes_then_repository_is_not_called()
    {
        var repository = new RecordingRepository();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new CreateProductUseCase(repository)
            .ExecuteAsync(new CreateProductInput("Product", -1), CancellationToken.None));
        Assert.Equal(0, repository.Saves);
    }

    private static HttpClient Client(IServiceProvider services)
    {
        var server = services.GetRequiredService<IServer>();
        return new HttpClient { BaseAddress = new Uri(server.Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
    }

    private sealed class RecordingRepository : IProductRepository
    {
        public int Saves { get; private set; }
        public Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Product?>(null);
        public Task SaveAsync(Product aggregate, CancellationToken cancellationToken = default) { Saves++; return Task.CompletedTask; }
    }
}
