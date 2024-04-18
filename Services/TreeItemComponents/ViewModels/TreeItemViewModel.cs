﻿namespace PaxDeiBot.Services.TreeItemComponents.ViewModels;

public class TreeItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ulong Count { get; set; }
    public List<TreeItemViewModel> Components { get; set; }
}