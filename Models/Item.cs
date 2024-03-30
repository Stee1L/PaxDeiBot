using System.ComponentModel.DataAnnotations;

namespace PaxDeiBot.Models;

public class Item
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public ICollection<ItemComponent> Component { get; set; }
}

public class ItemComponent
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int ComponentId { get; set; }
    public int Quantity { get; set; }

    public Item Item { get; set; }
    public Item Component { get; set; }
}