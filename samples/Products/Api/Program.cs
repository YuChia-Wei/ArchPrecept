using Products.Sample;

await ProductApi.Create(args).RunAsync();

namespace Products.Sample
{
    public static class ProductApi
    {
        public static WebApplication Create(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers().AddApplicationPart(typeof(ProductsController).Assembly);
            builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
            builder.Services.AddScoped<ICreateProductUseCase, CreateProductUseCase>();
            builder.Services.AddScoped<IGetProductUseCase, GetProductUseCase>();
            var app = builder.Build();
            app.MapControllers();
            return app;
        }
    }
}
