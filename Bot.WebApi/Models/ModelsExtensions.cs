using PaxDeiBot.Controllers.ViewModels;

namespace PaxDeiBot.Models;

public static class ModelsExtensions
{
    public static ItemShortViewModel ToShortViewModel(this Item item)
    {
        return new ItemShortViewModel()
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public static ItemFullViewModel ToFullViewModel(this Item item, IEnumerable<Item> children)
    {
        return new ItemFullViewModel()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Components = children.Select(f => f.ToShortViewModel()).ToList(),
        };
    }
}