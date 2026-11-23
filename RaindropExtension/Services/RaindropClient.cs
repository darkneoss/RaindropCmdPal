using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaindropExtension;

internal sealed class RaindropUnauthorizedException : Exception
{
}

internal sealed class RaindropClient
{
    private static readonly HttpClient Client = new();

    public async Task<IReadOnlyList<Raindrop>> SearchAsync(string query, string token, CancellationToken ct)
    {
        var url = "https://api.raindrop.io/rest/v1/raindrops/0?search=" + Uri.EscapeDataString(query);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await Client.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new RaindropUnauthorizedException();
        }

        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return document.RootElement.GetProperty("items").EnumerateArray().Select(ParseItem).ToArray();
    }

    private static string Text(JsonElement item, string name) =>
        item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static Raindrop ParseItem(JsonElement item)
    {
        var tags = item.TryGetProperty("tags", out var tagList) && tagList.ValueKind == JsonValueKind.Array
            ? tagList.EnumerateArray()
                .Where(t => t.ValueKind == JsonValueKind.String)
                .Select(t => t.GetString() ?? "")
                .ToArray()
            : [];
        return new Raindrop(
            item.GetProperty("_id").ToString(),
            Text(item, "title"),
            Text(item, "excerpt"),
            Text(item, "link"),
            Text(item, "cover"),
            Text(item, "domain"),
            tags);
    }
}
