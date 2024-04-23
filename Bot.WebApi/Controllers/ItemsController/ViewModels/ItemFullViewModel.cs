namespace PaxDeiBot.Controllers.ViewModels;

public class ItemFullViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ItemShortViewModel> Components { get; set; }
}