using PaxDeiBot.Models;
using PaxDeiBot.Services.TreeItemComponents.ViewModels;

namespace PaxDeiBot.Services.TreeItemComponents;

public static class ToTreeItemExtensions
{
    public static TreeItemViewModel ToTreeItem(this Item item, ulong count, uint countMultiplier) =>
        new()
        {
            Name = item.Name,
            Description = item.Description,
            Id = item.Id,
            Count = count * countMultiplier,
            Components = new List<TreeItemViewModel>()
        };
}