namespace PaxDeiBotApp.Models;

public class ItemFullModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ItemShortModel> Components { get; set; } 
}