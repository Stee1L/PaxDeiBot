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

    public static List<TreeItemViewModel> SelectPrimal(this TreeItemViewModel tree)
    {
        var list = new List<TreeItemViewModel>();

        if (tree.Components.Any())
        {
            foreach (var component in tree.Components)
            {
                list.AddRange(component.SelectPrimal());
            }
        }
        else
        {
            list.Add(tree);
        }
        
        return list;
    }

    public static PrimalComponentViewModel ToPrimalComponentViewModel(this TreeItemViewModel model) =>
        new PrimalComponentViewModel()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Count = model.Count
        };
}