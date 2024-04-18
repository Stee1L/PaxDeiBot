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

    public async Task<TreeItemViewModel> GetTree(Guid id, uint countMultiplier)
    {
        var item = _context.Items.FirstOrDefault(f => f.Id == id)
                   ?? throw new Exception("Не найден указанный предмет");

        var tree = item.ToTreeItem(1, countMultiplier);

        await FillTree(tree, countMultiplier);

        return tree;
    }

    public async Task<List<PrimalComponentViewModel>> GetPrimalComponents(Guid id, uint multiplier)
    {
        var tree = await GetTree(id, multiplier);

        return tree
            .SelectPrimal()
            .Select(f => f.ToPrimalComponentViewModel())
            .ToList();
    }

    private async Task FillTree(TreeItemViewModel model, uint countMultiplier)
    {
        var children = _context.Items.Join(_context.ItemComponents.Where(f => f.ParentId == model.Id), item => item.Id,
            component => component.ChildId, (item, component) => new {item, component.Count});
        model.Components = children.Select(f => f.item.ToTreeItem(f.Count, countMultiplier)).ToList();

        foreach (var component in model.Components)
        {
            await FillTree(component, countMultiplier);
        }
    }
}