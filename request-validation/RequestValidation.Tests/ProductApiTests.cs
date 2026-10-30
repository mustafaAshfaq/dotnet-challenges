using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RequestValidation.Tests;

public sealed class ProductApiTests
{
    [Fact]
    public async Task Whitespace_name_with_quantity_0_returns_400_and_stores_nothing()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var response = await client.PostAsJsonAsync(
            "/products",
            new { name = " ", quantity = 0 });

        await CollectValidationProblem(
            response,
            missing,
            "POST /products with name \" \" and quantity 0");
        await CollectCollection(
            client,
            missing,
            expectedCount: 0,
            effect: "GET /products must return an empty collection after that rejected create.");

        AssertNoMissingEffects(missing);
    }

    [Fact]
    public async Task Empty_name_with_quantity_101_returns_400_and_stores_nothing()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var response = await client.PostAsJsonAsync(
            "/products",
            new { name = "", quantity = 101 });

        await CollectValidationProblem(
            response,
            missing,
            "POST /products with an empty name and quantity 101");
        await CollectCollection(
            client,
            missing,
            expectedCount: 0,
            effect: "GET /products must return an empty collection after that rejected create.");

        AssertNoMissingEffects(missing);
    }

    [Fact]
    public async Task Valid_create_and_replace_keep_success_statuses_and_invalid_replace_preserves_product()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var create = await client.PostAsJsonAsync(
            "/products",
            new { name = "a", quantity = 1 });
        if (create.StatusCode != HttpStatusCode.Created)
        {
            missing.Add(
                $"POST /products with name \"a\" and quantity 1 must return 201, but it returned {(int)create.StatusCode}.");
        }

        var stored = await CollectCollection(
            client,
            missing,
            expectedCount: 1,
            effect: "GET /products must return one product with name \"a\" and quantity 1.");
        if (stored is { Count: 1 } && (stored[0].Name != "a" || stored[0].Quantity != 1))
        {
            missing.Add(
                "GET /products must return one product with name \"a\" and quantity 1.");
        }

        if (stored is { Count: 1 })
        {
            var id = stored[0].Id;

            using var invalidReplace = await client.PutAsJsonAsync(
                $"/products/{id}",
                new { name = " ", quantity = 0 });
            await CollectValidationProblem(
                invalidReplace,
                missing,
                $"PUT /products/{id} with name \" \" and quantity 0");

            var preserved = await ReadProduct(client, id);
            if (preserved is null || preserved.Name != "a" || preserved.Quantity != 1)
            {
                missing.Add(
                    $"GET /products/{id} must still return name \"a\" and quantity 1 after the rejected replace.");
            }

            using var validReplace = await client.PutAsJsonAsync(
                $"/products/{id}",
                new { name = "a", quantity = 100 });
            if (validReplace.StatusCode != HttpStatusCode.NoContent)
            {
                missing.Add(
                    $"PUT /products/{id} with name \"a\" and quantity 100 must return 204, but it returned {(int)validReplace.StatusCode}.");
            }

            var updated = await ReadProduct(client, id);
            if (updated is null || updated.Name != "a" || updated.Quantity != 100)
            {
                missing.Add(
                    $"GET /products/{id} must return name \"a\" and quantity 100 after the valid replace.");
            }
        }
        else
        {
            missing.Add(
                "The replace effects were not observed because the create did not leave one stored product.");
        }

        AssertNoMissingEffects(missing);
    }

    [Fact]
    public async Task Unknown_product_returns_404()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/products/9999");

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound,
            $"GET /products/{{id}} for an id that is not stored must return 404, but it returned {(int)response.StatusCode}.");
    }

    private static async Task CollectValidationProblem(
        HttpResponseMessage response,
        List<string> missing,
        string request)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            missing.Add(
                $"{request} must return 400, but it returned {(int)response.StatusCode}.");
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
        {
            missing.Add(
                $"{request} must return media type application/problem+json, but it returned \"{mediaType ?? "none"}\".");
        }

        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        }
        catch (JsonException)
        {
            missing.Add($"{request} must return an errors entry for name.");
            missing.Add($"{request} must return an errors entry for quantity.");
            return;
        }

        using (document)
        {
            if (!TryGetProperty(document.RootElement, "errors", out var errors)
                || errors.ValueKind != JsonValueKind.Object)
            {
                missing.Add($"{request} must return an errors entry for name.");
                missing.Add($"{request} must return an errors entry for quantity.");
                return;
            }

            if (!HasError(errors, "name"))
            {
                missing.Add($"{request} must return an errors entry for name.");
            }

            if (!HasError(errors, "quantity"))
            {
                missing.Add($"{request} must return an errors entry for quantity.");
            }
        }
    }

    private static async Task<List<ProductResponse>?> CollectCollection(
        HttpClient client,
        List<string> missing,
        int expectedCount,
        string effect)
    {
        using var response = await client.GetAsync("/products");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            missing.Add(
                $"{effect} The collection request returned {(int)response.StatusCode}.");
            return null;
        }

        try
        {
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
            if (products is null || products.Count != expectedCount)
            {
                missing.Add(effect);
                return products;
            }

            return products;
        }
        catch (JsonException)
        {
            missing.Add(effect);
            return null;
        }
    }

    private static async Task<ProductResponse?> ReadProduct(HttpClient client, int id)
    {
        using var response = await client.GetAsync($"/products/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<ProductResponse>();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool HasError(JsonElement errors, string expectedName) =>
        errors.EnumerateObject().Any(property =>
            string.Equals(property.Name, expectedName, StringComparison.OrdinalIgnoreCase));

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static void AssertNoMissingEffects(List<string> missing)
    {
        Assert.True(
            missing.Count == 0,
            "Missing effects:" + Environment.NewLine + string.Join(Environment.NewLine, missing));
    }

    private sealed record ProductResponse(int Id, string Name, int Quantity);
}
