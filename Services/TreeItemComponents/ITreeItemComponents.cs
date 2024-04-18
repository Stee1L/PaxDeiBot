using PaxDeiBot.Services.TreeItemComponents.ViewModels;

namespace PaxDeiBot.Services.TreeItemComponents;

public interface ITreeItemComponents
{
    public Task<TreeItemViewModel> GetTree(Guid id, uint multiplier);
    public Task<List<PrimalComponentViewModel>> GetPrimalComponents(Guid id, uint multiplier);
}