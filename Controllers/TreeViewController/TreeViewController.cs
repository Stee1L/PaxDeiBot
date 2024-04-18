using Microsoft.AspNetCore.Mvc;
using PaxDeiBot.Services.TreeItemComponents;
using PaxDeiBot.Services.TreeItemComponents.ViewModels;

namespace PaxDeiBot.Controllers.TreeViewController;

// [Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TreeViewController : ControllerBase
{
    private readonly ITreeItemComponents _treeItemComponents;

    public TreeViewController(ITreeItemComponents treeItemComponents)
    {
        _treeItemComponents = treeItemComponents;
    }

    [HttpGet("/tree/{id:guid}/count/{multiplier:int}")]
    public Task<TreeItemViewModel> GetTree(Guid id, uint multiplier) => 
        _treeItemComponents.GetTree(id, multiplier);
}