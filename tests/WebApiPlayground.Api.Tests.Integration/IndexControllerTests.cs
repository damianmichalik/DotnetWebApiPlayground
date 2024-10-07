using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApiPlayground.Api.Tests.Integration;

public class IndexControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IndexControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async void It_Returns_Correct_Status_Code_On_Main_Route()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}