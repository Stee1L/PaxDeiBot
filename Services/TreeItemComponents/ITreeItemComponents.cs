using PaxDeiBot.Services.TreeItemComponents.ViewModels;

namespace PaxDeiBot.Services.TreeItemComponents;

public interface ITreeItemComponents
{
    public Task<TreeItemViewModel> GetTree(Guid id, uint multiplier);
}