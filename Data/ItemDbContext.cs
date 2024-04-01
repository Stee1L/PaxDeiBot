using Microsoft.EntityFrameworkCore;
using PaxDeiBot.Models;
namespace PaxDeiBot.Data;

public class ItemDbContext: DbContext
{
    public ItemDbContext(DbContextOptions<ItemDbContext> options)
        : base (options)
    {
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<ItemComponent> ItemComponents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<Item>().HasMany(f => f.Components);
        // modelBuilder.Entity<Item>().HasMany(f => f.ComponentOfItems);
        // modelBuilder.Entity<Item>().Navigation(f=>f.ComponentOfItems).AutoInclude();
        // modelBuilder.Entity<Item>().Navigation(f=>f.Components).AutoInclude();
        
        base.OnModelCreating(modelBuilder);
    }
}