using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiPlayground.Api.Entities;
using WebApiPlayground.Api.Infrastructure;

namespace WebApiPlayground.Api.Tests.Integration;

public class PostsControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;

    private AppDbContext _dbContext;

    public PostsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _dbContext.Database.Migrate();
    }

    [Fact]
    public async void It_Returns_Correct_Status_Code_On_Main_Route()
    {
        await _dbContext.Posts.AddAsync(new Post
        {
            Title = "Post Title 1",
            Content = "Post Content 1"
        });
        await _dbContext.Posts.AddAsync(new Post
        {
            Title = "Post Title 2",
            Content = "Post Content 2"
        });
        await _dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<PostDTO[]>("/posts");
        
        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _dbContext?.Dispose();
    }
}