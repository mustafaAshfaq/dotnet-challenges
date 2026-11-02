using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InputTrust.Tests;

public sealed class ListingApiTests
{
    [Fact]
    public async Task Create_with_sent_price_stores_title_and_keeps_price_at_20()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var create = await client.PostAsJsonAsync(
            "/listings",
            new { title = "Lamp", price = 1 });
        var created = await ReadListing(create);
        if (create.StatusCode != HttpStatusCode.Created)
        {
            missing.Add(
                $"POST /listings with {{ \"title\": \"Lamp\", \"price\": 1 }} must return 201, but it returned {(int)create.StatusCode}.");
        }

        if (created is not { Title: "Lamp", PriceIs20: true, Id: not null })
        {
            missing.Add(
                """POST /listings with { "title": "Lamp", "price": 1 } must return JSON whose title is "Lamp" and whose price is 20, with an id.""");
        }

        if (created?.Id is { } id)
        {
            using var read = await client.GetAsync($"/listings/{Uri.EscapeDataString(id)}");
            var stored = await ReadListing(read);
            if (read.StatusCode != HttpStatusCode.OK
                || stored is not { Title: "Lamp", PriceIs20: true }
                || stored.Id != id)
            {
                missing.Add(
                    """GET /listings/{id} must return 200 and the same title "Lamp", price 20, and id.""");
            }
        }
        else
        {
            missing.Add(
                """GET /listings/{id} must return 200 and the same title "Lamp", price 20, and id.""");
        }

        AssertNoMissingEffects(missing);
    }

    [Fact]
    public async Task Create_without_price_stores_title_and_keeps_price_at_20()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var create = await client.PostAsJsonAsync(
            "/listings",
            new { title = "Lamp" });
        var created = await ReadListing(create);
        if (create.StatusCode != HttpStatusCode.Created)
        {
            missing.Add(
                $"POST /listings with {{ \"title\": \"Lamp\" }} must return 201, but it returned {(int)create.StatusCode}.");
        }

        if (created is not { Title: "Lamp", PriceIs20: true })
        {
            missing.Add(
                """POST /listings with { "title": "Lamp" } must return 201 with title "Lamp" and price 20.""");
        }

        if (created?.Id is { } id)
        {
            using var read = await client.GetAsync($"/listings/{Uri.EscapeDataString(id)}");
            var stored = await ReadListing(read);
            if (read.StatusCode != HttpStatusCode.OK
                || stored is not { Title: "Lamp", PriceIs20: true })
            {
                missing.Add(
                    """GET /listings/{id} must return title "Lamp" and price 20.""");
            }
        }
        else
        {
            missing.Add(
                """GET /listings/{id} must return title "Lamp" and price 20.""");
        }

        AssertNoMissingEffects(missing);
    }

    [Fact]
    public async Task Replace_with_sent_price_stores_title_and_keeps_price_at_20()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var create = await client.PostAsJsonAsync(
            "/listings",
            new { title = "Lamp" });
        var created = await ReadListing(create);
        if (create.StatusCode != HttpStatusCode.Created || created is not { PriceIs20: true, Id: not null })
        {
            missing.Add(
                """POST /listings with { "title": "Lamp" } must return 201 with price 20.""");
        }

        if (created?.Id is { } id)
        {
            using var replace = await client.PutAsJsonAsync(
                $"/listings/{Uri.EscapeDataString(id)}",
                new { title = "Shade", price = 1 });
            if (replace.StatusCode != HttpStatusCode.NoContent)
            {
                missing.Add(
                    $"PUT /listings/{{id}} with {{ \"title\": \"Shade\", \"price\": 1 }} must return 204, but it returned {(int)replace.StatusCode}.");
            }

            using var read = await client.GetAsync($"/listings/{Uri.EscapeDataString(id)}");
            var stored = await ReadListing(read);
            if (read.StatusCode != HttpStatusCode.OK
                || stored is not { Title: "Shade", PriceIs20: true })
            {
                missing.Add(
                    """PUT /listings/{id} with { "title": "Shade", "price": 1 } must store title "Shade" and price 20.""");
            }
        }
        else
        {
            missing.Add(
                """PUT /listings/{id} with { "title": "Shade", "price": 1 } must store title "Shade" and price 20.""");
        }

        AssertNoMissingEffects(missing);
    }

    [Fact]
    public async Task Replace_without_price_stores_title_and_keeps_price_at_20()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var missing = new List<string>();

        using var create = await client.PostAsJsonAsync(
            "/listings",
            new { title = "Lamp", price = 1 });
        var created = await ReadListing(create);
        if (create.StatusCode != HttpStatusCode.Created || created is not { PriceIs20: true, Id: not null })
        {
            missing.Add(
                """POST /listings with { "title": "Lamp", "price": 1 } must return 201 with price 20.""");
        }

        if (created?.Id is { } id)
        {
            using var replace = await client.PutAsJsonAsync(
                $"/listings/{Uri.EscapeDataString(id)}",
                new { title = "Shade" });
            if (replace.StatusCode != HttpStatusCode.NoContent)
            {
                missing.Add(
                    $"PUT /listings/{{id}} with {{ \"title\": \"Shade\" }} must return 204, but it returned {(int)replace.StatusCode}.");
            }

            using var read = await client.GetAsync($"/listings/{Uri.EscapeDataString(id)}");
            var stored = await ReadListing(read);
            if (read.StatusCode != HttpStatusCode.OK
                || stored is not { Title: "Shade", PriceIs20: true })
            {
                missing.Add(
                    """PUT /listings/{id} with { "title": "Shade" } must store title "Shade" and price 20.""");
            }
        }
        else
        {
            missing.Add(
                """PUT /listings/{id} with { "title": "Shade" } must store title "Shade" and price 20.""");
        }

        AssertNoMissingEffects(missing);
    }

    private static async Task<ListingObservation?> ReadListing(HttpResponseMessage response)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            string? id = null;
            if (TryGetProperty(document.RootElement, "id", out var idElement))
            {
                id = idElement.ValueKind switch
                {
                    JsonValueKind.String => idElement.GetString(),
                    JsonValueKind.Number => idElement.GetRawText(),
                    _ => null,
                };
                if (string.IsNullOrEmpty(id))
                {
                    id = null;
                }
            }

            string? title = null;
            if (TryGetProperty(document.RootElement, "title", out var titleElement)
                && titleElement.ValueKind == JsonValueKind.String)
            {
                title = titleElement.GetString();
            }

            var priceIs20 = TryGetProperty(document.RootElement, "price", out var priceElement)
                && priceElement.ValueKind == JsonValueKind.Number
                && priceElement.TryGetDecimal(out var price)
                && price == 20m;

            return new ListingObservation(id, title, priceIs20);
        }
        catch (JsonException)
        {
            return null;
        }
    }

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

    private sealed record ListingObservation(string? Id, string? Title, bool PriceIs20);
}
