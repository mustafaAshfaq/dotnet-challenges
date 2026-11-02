using InputTrust.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ListingStore>();

var app = builder.Build();

app.MapGet("/listings/{id:int}", (int id, ListingStore store) =>
{
    IResult response = store.Get(id) is { } listing
        ? Results.Ok(listing)
        : Results.NotFound();

    return response;
});

app.MapPost("/listings", (Listing input, ListingStore store) =>
{
    var listing = store.Create(input);
    return Results.Created($"/listings/{listing.Id}", listing);
});

app.MapPut("/listings/{id:int}", (int id, Listing input, ListingStore store) =>
{
    IResult response = store.Replace(id, input)
        ? Results.NoContent()
        : Results.NotFound();

    return response;
});

app.Run();

public partial class Program { }
