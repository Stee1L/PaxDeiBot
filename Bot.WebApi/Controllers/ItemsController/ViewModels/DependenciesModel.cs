namespace PaxDeiBot.Controllers.ViewModels;

public class DependenciesModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<DependenciesModel> Components { get; set; }
    public List<DependenciesModel> NeedForParents { get; set; }
}