using Microsoft.EntityFrameworkCore;
using PaxDeiBot.Models;
namespace PaxDeiBot.Data;

public class ItemDbContext: DbContext
{
    private readonly IConfiguration _configuration;

    public ItemDbContext(DbContextOptions<ItemDbContext> options, IConfiguration configuration)
        : base (options)
    {
        _configuration = configuration;
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<ItemComponent> ItemComponents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemComponent>()
            .HasOne(ic => ic.Item)
            .WithMany(i => i.Component)
            .HasForeignKey(ic => ic.ItemId);

        modelBuilder.Entity<ItemComponent>()
            .HasOne(ic => ic.Component)
            .WithMany()
            .HasForeignKey(ic => ic.ComponentId);
    }
}