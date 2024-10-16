using Microsoft.AspNetCore.Mvc;
using WebApiPlayground.Api.Entities;
using WebApiPlayground.Api.Infrastructure;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PostsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDTO[]>> Get()
    {
        return Ok(_dbContext.Posts.Select(x => PostToDTO(x)));
    }

    private static PostDTO PostToDTO(Post post) =>
       new PostDTO
       {
           Id = post.Id,
           Title = post.Title,
           Content = post.Content
       };
}