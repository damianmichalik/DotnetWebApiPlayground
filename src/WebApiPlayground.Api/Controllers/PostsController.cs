using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPlayground.Api.Dto;
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
    public async Task<ActionResult<IEnumerable<PostDTO>>> Get()
    {
        return await _dbContext.Posts.Select(p => PostToDTO(p)).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDTO>> Get(int id)
    {
        var post = await _dbContext.Posts.FindAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        return PostToDTO(post);
    }

    [HttpPost]
    public async Task<ActionResult<PostDTO>> Post(CreatePostDTO post)
    {
        var newPost = new Post
        {
            Title = post.Title,
            Content = post.Content
        };

        _dbContext.Posts.Add(newPost);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = newPost.Id }, PostToDTO(newPost));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var postItem = await _dbContext.Posts.FindAsync(id);

        if (postItem == null)
        {
            return NotFound();
        }

        _dbContext.Posts.Remove(postItem);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PostDTO>> Put(int id, UpdatePostDTO post)
    {
        var postItem = await _dbContext.Posts.FindAsync(id);

        if (postItem == null)
        {
            return NotFound();
        }

        postItem.Title = post.Title;
        postItem.Content = post.Content;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!PostItemExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool PostItemExists(int id)
    {
        return _dbContext.Posts.Any(p => p.Id == id);
    }

    private static PostDTO PostToDTO(Post post) =>
       new PostDTO
       {
           Id = post.Id,
           Title = post.Title,
           Content = post.Content
       };
}