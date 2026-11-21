using SampleShop.Application;

try
{
    var input = new CreateProductInput(string.Join(" ", args));
    var created = await new CreateProductUseCase().ExecuteAsync(input, CancellationToken.None);
    Console.WriteLine($"Created: {created.Name}");
    return 0;
}
catch (ArgumentException error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}
