using System;
using System.Collections.Generic;

namespace AvaloniaApplication1.Models;

public class ItemFullModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ItemShortModel> Children { get; set; }
    public List<ItemShortModel> Parents { get; set; }
}