using Microsoft.EntityFrameworkCore;
using WebApiPlayground.Api.Entities;

namespace WebApiPlayground.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    public DbSet<Post> Posts { get; set; }
}