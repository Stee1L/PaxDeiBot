﻿namespace PaxDeiBot.Models;

public class ItemComponent
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public Guid ChildId { get; set; }
    public ulong Count { get; set; }
}