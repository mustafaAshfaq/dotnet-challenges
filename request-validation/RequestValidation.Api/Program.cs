using RequestValidation.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ProductStore>();

var app = builder.Build();

app.MapGet("/products", (ProductStore store) => Results.Ok(store.GetAll()));

app.MapGet("/products/{id:int}", (int id, ProductStore store) =>
{
    IResult response = store.Get(id) is { } product
        ? Results.Ok(product)
        : Results.NotFound();

    return response;
});

app.MapPost("/products", (ProductInput input, ProductStore store) =>
{
    var product = store.Create(input);
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:int}", (int id, ProductInput input, ProductStore store) =>
{
    IResult response = store.Replace(id, input)
        ? Results.NoContent()
        : Results.NotFound();

    return response;
});

app.Run();

public partial class Program { }
