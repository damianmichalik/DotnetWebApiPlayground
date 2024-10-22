using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WebApiPlayground.Api.Dto;
using WebApiPlayground.Api.Entities;
using WebApiPlayground.Api.Infrastructure;

namespace WebApiPlayground.Api.Tests.Integration;

public class PostsControllerTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private AppDbContext _dbContext;

    public PostsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();        
    }

    [Fact]
    public async void It_Returns_Posts()
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

    [Fact]
    public async void It_Returns_Post_By_Id()
    {
        await _dbContext.Posts.AddAsync(new Post
        {
            Id = 1,
            Title = "Post Title 1",
            Content = "Post Content 1"
        });
        await _dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        var response = await client.GetFromJsonAsync<PostDTO>("/posts/1");
        
        Assert.NotNull(response);
        Assert.Equal(1, response.Id);
        Assert.Equal("Post Title 1", response.Title);
        Assert.Equal("Post Content 1", response.Content);
    }

    [Fact]
    public async void It_Updates_Post()
    {
        await _dbContext.Posts.AddAsync(new Post
        {
            Id = 1,
            Title = "Old Post Title",
            Content = "Old Post Content"
        });
        await _dbContext.SaveChangesAsync();

        var updatedPost = new UpdatePostDTO
        {
            Title = "New Post Title",
            Content = "New Post Content"
        };

        var payload = new StringContent(
            JsonSerializer.Serialize(updatedPost), 
            Encoding.UTF8, 
            "application/json"
        );

        var client = _factory.CreateClient();
        var response = await client.PutAsync("/posts/1", payload);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var post = await _dbContext.Posts.FindAsync(1);
        if (post is not null) 
        {
            _dbContext.Entry(post).Reload();
        }

        Assert.Equal("New Post Title", post?.Title);
        Assert.Equal("New Post Content", post?.Content);
        Assert.Equal(1, _dbContext.Posts.Count());
    }

    [Fact]
    public async void It_Creates_New_Post()
    {
        var newPost = new CreatePostDTO
        {
            Title = "New Post Title",
            Content = "New Post Content"
        };

        var payload = new StringContent(
            JsonSerializer.Serialize(newPost), 
            Encoding.UTF8, 
            "application/json"
        );

        var client = _factory.CreateClient();
        var response = await client.PostAsync("/posts", payload);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var createdPost = JsonSerializer.Deserialize<PostDTO>(jsonResponse, options);
        Console.WriteLine(createdPost);

        Assert.Equal("New Post Title", createdPost?.Title);
        Assert.Equal("New Post Content", createdPost?.Content);
        Assert.Equal(1, _dbContext.Posts.Count());
    }

    [Fact]
    public async void It_Deletes_Post()
    {
        await _dbContext.Posts.AddAsync(new Post
        {
            Id = 1,
            Title = "Post Title",
            Content = "Post Content"
        });
        await _dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        var response = await client.DeleteAsync("/posts/1");
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, _dbContext.Posts.Count());
    }

    public Task DisposeAsync() =>  _factory.ResetDatabase();

    public Task InitializeAsync() => Task.CompletedTask;
}