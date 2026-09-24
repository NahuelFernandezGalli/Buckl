using System.Text;
using System.Text.Json;

namespace Buckl.Api.Tests;

internal static class Http
{
    public static Uri Url(string relative) => new(relative, UriKind.Relative);

    public static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    public static async Task<JsonElement> ReadJsonAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        return document.RootElement.Clone();
    }

    public static async Task<string?> ReadProblemCodeAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        (await response.ReadJsonAsync(cancellationToken)).GetProperty("code").GetString();
}
