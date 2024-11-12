using CommunityToolkit.Aspire.Testing;
using Aspire.Components.Common.Tests;
using FluentAssertions;
using OllamaSharp;

namespace CommunityToolkit.Aspire.Hosting.Ollama.Tests;

[RequiresDocker]
public class AppHostTests(AspireIntegrationTestFixture<Projects.Ollama_AppHost> fixture) : IClassFixture<AspireIntegrationTestFixture<Projects.Ollama_AppHost>>
{
    [Fact]
    public async Task ResourceStartsAndRespondsOk()
    {
        var resourceName = "ollama";
        await fixture.ResourceNotificationService.WaitForResourceAsync(resourceName, KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(5));
        var httpClient = fixture.CreateHttpClient(resourceName);

        var response = await httpClient.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OllamaListsAvailableModels()
    {
        var distributedAppModel = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var modelResources = distributedAppModel.Resources.OfType<OllamaModelResource>();
        var rns = fixture.ResourceNotificationService;

        await Task.WhenAll([
                rns.WaitForResourceHealthyAsync("ollama"),
                .. modelResources.Select(m => rns.WaitForResourceHealthyAsync(m.Name))
            ]).WaitAsync(TimeSpan.FromMinutes(5));
        var httpClient = fixture.CreateHttpClient("ollama");

        var models = await new OllamaApiClient(httpClient).ListLocalModelsAsync();

        models.Should().NotBeEmpty();
        models.Should().HaveCount(modelResources.Count());
    }
}
