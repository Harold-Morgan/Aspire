using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CommunityToolkit.Aspire.Hosting.Garage;

/// <summary>
/// A health check that ensures the Garage cluster layout is initialized.
/// On first successful connection it assigns the single-node layout and applies it.
/// Subsequent calls simply verify the admin API is responsive.
/// </summary>
internal sealed class GarageLayoutHealthCheck(Func<string> getAdminUrl, Func<CancellationToken, ValueTask<string?>> getAdminToken) : IHealthCheck
{
    private volatile bool _layoutInitialized;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await getAdminToken(cancellationToken).ConfigureAwait(false);
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(getAdminUrl()),
                Timeout = TimeSpan.FromSeconds(5),
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!_layoutInitialized)
            {
                await InitializeLayoutAsync(httpClient, cancellationToken).ConfigureAwait(false);
                _layoutInitialized = true;
            }

            // Verify the admin API is still responding
            var response = await httpClient.GetAsync("/v2/GetClusterStatus", cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Admin API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Garage admin API is not available", ex);
        }
    }

    private static async Task InitializeLayoutAsync(HttpClient httpClient, CancellationToken ct)
    {
        // Step 1: Get the node ID
        var statusDoc = await httpClient.GetFromJsonAsync<JsonElement>("/v2/GetClusterStatus", ct).ConfigureAwait(false);
        var nodeId = statusDoc.GetProperty("nodes").EnumerateArray()
            .First()
            .GetProperty("id").GetString()!;

        // Step 2: Assign the node a storage role
        var assignResponse = await httpClient.PostAsJsonAsync("/v2/UpdateClusterLayout", new
        {
            roles = new[]
            {
                new
                {
                    id = nodeId,
                    zone = "dc1",
                    capacity = 1073741824, // 1 GiB
                    tags = Array.Empty<string>(),
                }
            }
        }, ct).ConfigureAwait(false);
        assignResponse.EnsureSuccessStatusCode();

        // Step 3: Apply the layout
        var applyResponse = await httpClient.PostAsJsonAsync("/v2/ApplyClusterLayout", new { version = 1 }, ct)
            .ConfigureAwait(false);
        applyResponse.EnsureSuccessStatusCode();
    }
}
