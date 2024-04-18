using PaxDeiBot.Data;
using PaxDeiBot.Services.TreeItemComponents.ViewModels;

namespace PaxDeiBot.Services.TreeItemComponents;

public class TreeItemComponents : ITreeItemComponents
{
    private readonly ItemDbContext _context;

    public TreeItemComponents(ItemDbContext context)
    {
        _context = context;
    }

    public Task<TreeItemViewModel> GetTree(Guid id)
    {
        throw new NotImplementedException();
    }

    private async Task<TreeItemViewModel> FillTree(TreeItemViewModel model)
    {
        var children = _context.Items.Join(_context.ItemComponents.Where(f => f.ParentId == model.Id), item => item.Id,
            component => component.ChildId, (item, component) => item);
        model.Components = children.Select(f => f.ToTreeItem()).ToList();

        foreach (var component in model.Components)
        {
            await FillTree(component);
        }

        return model;
    }
}