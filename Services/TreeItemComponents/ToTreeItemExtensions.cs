using PaxDeiBot.Models;
using PaxDeiBot.Services.TreeItemComponents.ViewModels;

namespace PaxDeiBot.Services.TreeItemComponents;

public static class ToTreeItemExtensions
{
    public static TreeItemViewModel ToTreeItem(this Item item) =>
        new()
        {
            Name = item.Name,
            Description = item.Description,
            Id = item.Id,
            Components = new List<TreeItemViewModel>()
        };
}